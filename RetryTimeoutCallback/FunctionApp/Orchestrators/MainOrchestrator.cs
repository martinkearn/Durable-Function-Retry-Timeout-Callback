using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FunctionApp.Activities;
using FunctionApp.Entities;
using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace FunctionApp.Orchestrators
{
    public static class MainOrchestrator
    {
        // Use these properties to control the behaviour of the function and api.

        /// <summary>
        /// Defines how long in milliseconds the API should wait before calling back.
        /// </summary>
        private const int _callbackAfterSeconds = 30;

        /// <summary>
        /// Api will return an error this % of the time. Use 0 if you never want an error, 100 if you always want an error.
        /// </summary>
        private const int _errorResponseLikelihoodPercentage = 0;

        /// <summary>
        /// How many seconds does the Api have to call back until the function times out
        /// </summary>
        private const int _timeoutLimitSeconds = 5;

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
            var callBackUrlBuilder = new StringBuilder(mainOrchestrationInput.SendEventPostUri.AbsoluteUri);
            callBackUrlBuilder.Replace(Constants.SendEventPostUriEventNameToken, Constants.CallbackEventName);
            callBackUrlBuilder.Replace(Constants.TempInstanceId, context.InstanceId);

            // Call Api until we get success or reach the retry limit
            AttemptCounterEntityState attemptCounterEntityState;
            HttpStatusCode status;
            var hasTimedOut = true;
            do
            {
                // Increment attempt counter
                context.SignalEntity(attemptCounterEntityId, "IncrementAttempts");

                // Trigger the Api with the CallApiActivityInput payload
                var callApiActivityInput = new CallApiActivityInput()
                {
                    CallbackUri = new Uri(callBackUrlBuilder.ToString()),
                    CallbackAfterSeconds = _callbackAfterSeconds,
                    ErrorResponseLikelihoodPercentage = _errorResponseLikelihoodPercentage,
                };
                status = await context.CallActivityAsync<HttpStatusCode>(nameof(CallApiActivity), callApiActivityInput);

                // Increment error count if status is anything other than ok
                if (status != HttpStatusCode.OK)
                {
                    context.SignalEntity(attemptCounterEntityId, "IncrementErrors");
                }

                // Wait for the api to call back
                var timeoutTimespan = new TimeSpan(0, 0, _timeoutLimitSeconds);
                try
                {
                    await context.WaitForExternalEvent<bool>(Constants.CallbackEventName, timeoutTimespan, default);
                    hasTimedOut = false;
                }
                catch (TimeoutException)
                {
                    // The api has failed to call back within the expected time span. Flag the timeout so the loop continues and increment the timeout counter
                    hasTimedOut = true;
                    context.SignalEntity(attemptCounterEntityId, "IncrementTimeouts");
                }

                // Get AttemptCounterEntityState
                attemptCounterEntityState = await context.CallEntityAsync<AttemptCounterEntityState>(attemptCounterEntityId, "Get");
            }
            while(ExecuteRetry(attemptCounterEntityState.AttemptsCount, status, hasTimedOut));

            return $"Finished with status of {status} after {attemptCounterEntityState.AttemptsCount} attempts, {attemptCounterEntityState.TimeoutsCount} timeouts, {attemptCounterEntityState.ErrorsCount} errors.";
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