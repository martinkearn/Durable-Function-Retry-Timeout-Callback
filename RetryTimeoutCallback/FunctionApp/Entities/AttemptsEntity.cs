using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FunctionApp.Models.AttemptsEntityState;
using static FunctionApp.Models.Enums;

namespace FunctionApp.Entities
{
    public class AttemptsEntity : IAttemptsEntity
    {
        [JsonProperty("attempts")]
        public Dictionary<Guid, Attempt> Attempts { get; set; }

        [JsonProperty("overallstate")]
        public string OverallState { get; set; }

        /// <summary>
        /// Main run method for entity.
        /// </summary>
        /// <param name="ctx">DurableEntityContext.</param>
        /// <returns>AttemptsEntity.</returns>
        [FunctionName(nameof(AttemptsEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<AttemptsEntity>();
        }

        /// <inheritdoc/>
        public void AddAttempt(Guid attemptId)
        {
            if (this.Attempts == default)
            {
                this.Attempts = new Dictionary<Guid, Attempt>();
            }

            var attempt = new Attempt()
            {
                Id = attemptId,
                Started = DateTime.UtcNow,
                StateSet = DateTime.UtcNow,
                TimeoutDue = null,
                State = Enums.AttemptState.New,
                Message = "New attempt",
                IsSuccess = false,
            };

            this.Attempts.Add(attemptId, attempt);
        }

        /// <inheritdoc/>
        public void UpdateAttempt(Attempt attempt)
        {
            // Remove the existing attempt(s) with this ID
            this.Attempts.Remove(attempt.Id);

            // Add new attempt
            attempt.StateSet = DateTime.UtcNow;
            attempt.IsSuccess = this.IsSuccess(attempt.State);
            this.Attempts.Add(attempt.Id, attempt);
        }

        /// <inheritdoc/>
        public void UpdateAttemptState(KeyValuePair<Guid, AttemptState> update)
        {
            var attempt = this.Attempts[update.Key];
            attempt.State = update.Value;
            this.UpdateAttempt(attempt);
        }

        /// <inheritdoc/>
        public void UpdateAttemptMessage(KeyValuePair<Guid, string> update)
        {
            var attempt = this.Attempts[update.Key];
            attempt.Message = update.Value;
            this.UpdateAttempt(attempt);
        }

        /// <inheritdoc/>
        public void UpdateAttemptTimeoutDue(KeyValuePair<Guid, DateTime> update)
        {
            var attempt = this.Attempts[update.Key];
            attempt.TimeoutDue = update.Value;
            this.UpdateAttempt(attempt);
        }

        /// <inheritdoc/>
        public void UpdateOverallState(string newState)
        {
            this.OverallState = newState;
        }

        /// <inheritdoc/>
        public Task<AttemptsEntityState> Get()
        {
            var state = new AttemptsEntityState()
            {
                Attempts = this.Attempts,
                OverallState = this.OverallState,
            };
            return Task.FromResult(state);
        }

        /// <summary>
        /// Convienence method to calculate whether the attempt is sucessfull or not based on the state.
        /// </summary>
        /// <param name="state">State on which to based the assessment of success.</param>
        /// <returns>Boolean indicating success or not.</returns>
        private bool IsSuccess(AttemptState state)
        {
            if ((state == AttemptState.ExecutedFailed) || (state == AttemptState.TimedOut) || (state == AttemptState.CallbackFailure))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
