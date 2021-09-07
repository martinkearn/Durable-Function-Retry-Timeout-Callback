using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FunctionApp.Activities;
using FunctionApp.Entities;
using FunctionApp.Helpers;
using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using static FunctionApp.Models.AttemptsEntityState;
using static FunctionApp.Models.Enums;

namespace FunctionApp.Orchestrators
{
    public static class MainOrchestrator
    {
        // Use these properties to control the behaviour of the function and api.

        /// <summary>
        /// Api will return an error this % of the time. Use 0 if you never want an error, 100 if you always want an error.
        /// </summary>
        private const int _errorResponseLikelihoodPercentage = 20;

        /// <summary>
        /// How many seconds does the Api have to call back until the function times out
        /// </summary>
        private const int _timeoutLimitSeconds = 60;

        /// <summary>
        /// How many times will the function attempt to call the api and receive an OK status code within the time span permitted.
        /// </summary>
        private const int _maxAttempts = 5;

        [FunctionName(nameof(MainOrchestrator))]
        public static async Task<string> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            // Get input
            var mainOrchestrationInput = context.GetInput<MainOrchestrationInput>();

            // Initiate AttemptsEntity and proxy
            var entityId = new EntityId(nameof(AttemptsEntity), context.InstanceId);
            var entity = context.CreateEntityProxy<IAttemptsEntity>(entityId);

            // Replace tokens in callback url with actual values
            var callBackUrlBuilder = new StringBuilder(mainOrchestrationInput.SendEventPostUri.ToString());
            callBackUrlBuilder.Replace(Constants.SendEventPostUriEventNameToken, Constants.CallbackEventName);
            callBackUrlBuilder.Replace(Constants.TempInstanceId, context.InstanceId);
            if (!context.IsReplaying)
            {
                Debug.WriteLine($"Callback uri:{callBackUrlBuilder}");
            }

            // Keep attempting until we meet the criteria to stop attempting
            AttemptsEntityState attemptsEntityState;
            Attempt mostRecentAttempt;
            entity.UpdateOverallState("Attempting API request and waiting for external system call back.");
            do
            {
                // Add new Attempt to entity
                var thisAttemptId = context.NewGuid();
                entity.AddAttempt(thisAttemptId);
                attemptsEntityState = await entity.Get();

                // Attempt the API call and wait for callback.
                try
                {
                    // Store status
                    entity.UpdateAttemptState(KeyValuePair.Create(thisAttemptId, AttemptState.Executing));

                    // Trigger the Api with the CallApiActivityInput payload
                    var callApiActivityInput = new CallApiActivityInput()
                    {
                        ErrorResponseLikelihoodPercentage = _errorResponseLikelihoodPercentage,
                        CallbackUri = new Uri(callBackUrlBuilder.ToString()),
                    };
                    (HttpStatusCode StatusCode, string StatusText) = await context.CallActivityAsync<(HttpStatusCode, string)>(nameof(CallApiActivity), callApiActivityInput);

                    // Store status
                    entity.UpdateAttemptState(KeyValuePair.Create(thisAttemptId, AttemptState.ExecutedSuccess));
                    entity.UpdateAttemptMessage(KeyValuePair.Create(thisAttemptId, "Request sent to API succesfully"));

                    // Wait for API to call back
                    try
                    {
                        var callbackTimeSpan = new TimeSpan(0, 0, _timeoutLimitSeconds);
                        entity.UpdateAttemptState(KeyValuePair.Create(thisAttemptId, AttemptState.WaitingForCallback));
                        entity.UpdateAttemptTimeoutDue(KeyValuePair.Create(thisAttemptId, context.CurrentUtcDateTime.Add(callbackTimeSpan)));

                        // This line will make the function wait for the callback event.
                        // This is a manual act that you can use postman or any http request tool for.
                        // Do a POST request to the value of callBackUrlBuilder.ToString() (see debug console). Attach a json body with the word "true" or "falase" in the body without any json structure.
                        bool callBackSuccess;
                        using (await context.LockAsync(entityId))
                        {
                            callBackSuccess = await context.WaitForExternalEvent<bool>(Constants.CallbackEventName, callbackTimeSpan, default);
                        }

                        var callbackOutcomeState = callBackSuccess ? AttemptState.CallbackSuccess : AttemptState.CallbackFailure;
                        entity.UpdateAttemptState(KeyValuePair.Create(thisAttemptId, callbackOutcomeState));
                        entity.UpdateAttemptMessage(KeyValuePair.Create(thisAttemptId, $"External system called back with {callbackOutcomeState}"));
                    }
                    catch (TimeoutException tex)
                    {
                        // The API has failed to call back within the expected time span. Flag the timeout so the loop continues and increment the timeout counter
                        entity.UpdateAttemptState(KeyValuePair.Create(thisAttemptId, AttemptState.TimedOut));
                        entity.UpdateAttemptMessage(KeyValuePair.Create(thisAttemptId, tex.Message));
                    }
                }
                catch (FunctionFailedException fex)
                {
                    // The attempt to call the API has failed (API returned an error code). Store status
                    entity.UpdateAttemptState(KeyValuePair.Create(thisAttemptId, AttemptState.ExecutedFailed));
                    entity.UpdateAttemptMessage(KeyValuePair.Create(thisAttemptId, fex.Message));
                }

                // Get AttemptsEntityState
                attemptsEntityState = await entity.Get();
                mostRecentAttempt = attemptsEntityState.Attempts.OrderByDescending(a => a.Value.Started).First().Value;
            }
            while (RetryHelper.RequireRetry(attemptsEntityState.Attempts.Count, mostRecentAttempt.IsSuccess, _maxAttempts));

            // Write function output
            var completionMessage = $"Completed API request after {attemptsEntityState.Attempts.Count} attempts. Final state {mostRecentAttempt.State}, status text: {mostRecentAttempt.Message}";
            entity.UpdateOverallState(completionMessage);

            return completionMessage;
        }
    }
}