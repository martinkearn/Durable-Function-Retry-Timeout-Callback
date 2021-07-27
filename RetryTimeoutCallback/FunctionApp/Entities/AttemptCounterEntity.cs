using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FunctionApp.Models.AttemptCounterEntityState;

namespace FunctionApp.Entities
{
    public class AttemptCounterEntity
    {
        [JsonProperty("attempts")]
        public List<Attempt> Attempts { get; set; }

        [JsonProperty("overallstate")]
        public string OverallState { get; set; }

        public void AddAttempt(Attempt newAttempt)
        {
            if (this.Attempts == default)
            {
                this.Attempts = new List<Attempt>();
            }

            newAttempt.DateTimeStateSet = DateTime.UtcNow;
            this.Attempts.Add(newAttempt);
        }

        public void UpdateAttempt(Attempt attempt)
        {
            // Remove the existing attempt(s) with this ID
            this.Attempts.RemoveAll(a => a.Id == attempt.Id);

            // Add new attempt
            attempt.DateTimeStateSet = DateTime.UtcNow;
            this.Attempts.Add(attempt);
        }

        public void UpdateOverallState(string newState)
        {
            this.OverallState = newState;
        }

        public Task Reset()
        {
            this.Attempts.Clear();
            return Task.CompletedTask;
        }

        public Task<AttemptCounterEntityState> Get()
        {
            var state = new AttemptCounterEntityState()
            {
                Attempts = this.Attempts,
                OverallState = this.OverallState,
            };
            return Task.FromResult(state);
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        [FunctionName(nameof(AttemptCounterEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<AttemptCounterEntity>();
        }

    }
}
