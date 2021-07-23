using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FunctionApp.Activities;
using FunctionApp.Entities;
using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using static FunctionApp.Models.AttemptCounterEntityState;

namespace FunctionApp.Orchestrators
{
    public static class MainOrchestrator
    {
        // Use these properties to control the behaviour of the function and api.

        /// <summary>
        /// Api will return an error this % of the time. Use 0 if you never want an error, 100 if you always want an error.
        /// </summary>
        private const int _errorResponseLikelihoodPercentage = 0;

        /// <summary>
        /// How many seconds does the Api have to call back until the function times out
        /// </summary>
        private const int _timeoutLimitSeconds = 30;

        /// <summary>
        /// How many times will the function attempt to call the api and receive an OK status code within the time span permitted.
        /// </summary>
        private const int _maxAttempts = 3;

        [FunctionName(nameof(MainOrchestrator))]
        public static async Task<string> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            // Get input
            var mainOrchestrationInput = context.GetInput<MainOrchestrationInput>();

            // Setup entity ID key'd from the orchestration instance id. One counter per orchestration instance.
            var attemptCounterEntityId = new EntityId(nameof(AttemptCounterEntity), context.InstanceId);

            // Replace tokens in callback url with actual values
            var callBackUrlBuilder = new StringBuilder(mainOrchestrationInput.SendEventPostUri.ToString());
            callBackUrlBuilder.Replace(Constants.SendEventPostUriEventNameToken, Constants.CallbackEventName);
            callBackUrlBuilder.Replace(Constants.TempInstanceId, context.InstanceId);
            if (!context.IsReplaying)
            {
                Debug.WriteLine($"Callback uri:{callBackUrlBuilder}");
            }

            // Call Api until we get success or reach the retry limit
            AttemptCounterEntityState attemptCounterEntityState;
            Attempt mostRecentAttempt;
            do
            {
                // Increment attempt counter
                var thisAttempt = new Attempt() 
                {
                    Id = context.NewGuid(),
                    DateTimeStarted = context.CurrentUtcDateTime,
                    State = "new",
                    StatusText = string.Empty,
                    StatusCode = null,
                };
                context.SignalEntity(attemptCounterEntityId, "AddAttempt", thisAttempt);

                // Trigger the Api with the CallApiActivityInput payload
                var callApiActivityInput = new CallApiActivityInput()
                {
                    ErrorResponseLikelihoodPercentage = _errorResponseLikelihoodPercentage,
                    CallbackUri = new Uri(callBackUrlBuilder.ToString()), 
                };
                (HttpStatusCode StatusCode, string StatusText) = await context.CallActivityAsync<(HttpStatusCode, string)>(nameof(CallApiActivity), callApiActivityInput);

                // Store status code and text for the attempt
                thisAttempt.StatusCode = StatusCode;
                thisAttempt.StatusText = StatusText;
                context.SignalEntity(attemptCounterEntityId, "UpdateAttempt", thisAttempt);
                
                // Wait for the api to call back
                try
                {
                    // This line will make the function wait for the callback event.
                    // This is a manual act that you can use postman or any api tool for.
                    // Do a POST request to the value of callBackUrlBuilder.ToString() (see debug console). Attach a json body with the word "true" in the body without any json structure.
                    await context.WaitForExternalEvent<bool>(Constants.CallbackEventName, new TimeSpan(0, 0, _timeoutLimitSeconds));
                    thisAttempt.State = "waiting";
                    context.SignalEntity(attemptCounterEntityId, "UpdateAttempt", thisAttempt);
                }
                catch (TimeoutException)
                {
                    // The api has failed to call back within the expected time span. Flag the timeout so the loop continues and increment the timeout counter
                    thisAttempt.State = "timedout";
                    context.SignalEntity(attemptCounterEntityId, "UpdateAttempt", thisAttempt);
                }

                // Get AttemptCounterEntityState
                attemptCounterEntityState = await context.CallEntityAsync<AttemptCounterEntityState>(attemptCounterEntityId, "Get");
                mostRecentAttempt = GetMostRecentAttempt(attemptCounterEntityState);
            }
            while(
            ExecuteRetry(
                attemptCounterEntityState.Attempts.Count, 
                (HttpStatusCode)mostRecentAttempt.StatusCode, 
                (mostRecentAttempt.State == "timedout")
            ));

            return $"Finished after {attemptCounterEntityState.Attempts.Count} attempts.";
        }

        private static bool ExecuteRetry(int attempts, HttpStatusCode status, bool timedOut)
        {
            var retry = false;

            // Retry if we do not have an OK response
            if (status != HttpStatusCode.OK) retry = true;

            // Retry if we timed out
            if (timedOut) retry = true;

            // Overrule other conditions if we are equal to max attempts count
            if (attempts == _maxAttempts) retry = false;

            return retry;
        }

        private static Attempt GetMostRecentAttempt(AttemptCounterEntityState state) => state.Attempts.OrderByDescending(a => a.DateTimeStarted).ToList().FirstOrDefault();

    }
}