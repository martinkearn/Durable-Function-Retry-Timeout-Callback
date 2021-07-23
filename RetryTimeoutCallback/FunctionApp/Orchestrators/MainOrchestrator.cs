using System;
using System.Diagnostics;
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
        private const int _timeoutLimitSeconds = 15;

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
            Debug.WriteLine($"Callback uri:{callBackUrlBuilder}");

            // Call Api until we get success or reach the retry limit
            AttemptCounterEntityState attemptCounterEntityState;
            HttpStatusCode status;
            var hasTimedOut = true;
            do
            {
                // Increment attempt counter
                var thisAttempt = new Attempt() 
                {
                    DateTimeStarted = context.CurrentUtcDateTime,
                    Order = default, // Will get over-written by the entity based on number of existing Attempt
                    State = "waiting",
                    StatusText = string.Empty,
                };
                context.SignalEntity(attemptCounterEntityId, "AddAttempt", thisAttempt);

                // Trigger the Api with the CallApiActivityInput payload
                var callApiActivityInput = new CallApiActivityInput()
                {
                    ErrorResponseLikelihoodPercentage = _errorResponseLikelihoodPercentage,
                    CallbackUri = new Uri(callBackUrlBuilder.ToString()), 
                };
                status = await context.CallActivityAsync<HttpStatusCode>(nameof(CallApiActivity), callApiActivityInput);

                // Increment error count if status is anything other than ok
                if (status != HttpStatusCode.OK)
                {
                    // TO DO update current attempt with error
                }

                // Wait for the api to call back
                try
                {
                    // This line will make the function wait for the callback event.
                    // This is a manual act that you can use postman or any api tool for.
                    // Do a POST request to the value of callBackUrlBuilder.ToString() (see debug console). Attach a json body with the word "true" in the body without any json structure.
                    await context.WaitForExternalEvent<bool>(Constants.CallbackEventName, new TimeSpan(0, 0, _timeoutLimitSeconds));
                    hasTimedOut = false;
                }
                catch (TimeoutException)
                {
                    // The api has failed to call back within the expected time span. Flag the timeout so the loop continues and increment the timeout counter
                    hasTimedOut = true;
                    // TO DO update current attempt with timeout
                }

                // Get AttemptCounterEntityState
                attemptCounterEntityState = await context.CallEntityAsync<AttemptCounterEntityState>(attemptCounterEntityId, "Get");

                // Write to console
                Debug.WriteLine($"Finished loop. Retry is {ExecuteRetry(attemptCounterEntityState.Attempts.Count, status, hasTimedOut)}. {attemptCounterEntityState.Attempts.Count} attempts.");
            }
            while(ExecuteRetry(attemptCounterEntityState.Attempts.Count, status, hasTimedOut));

            return $"Finished with status of {status} after {attemptCounterEntityState.Attempts.Count} attempts.";
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
    }
}