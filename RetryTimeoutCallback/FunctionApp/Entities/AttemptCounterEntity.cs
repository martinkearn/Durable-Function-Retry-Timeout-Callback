using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace FunctionApp.Entities
{
    public class AttemptCounterEntity
    {
        [JsonProperty("attempts")]
        public int Attempts { get; set; }

        [JsonProperty("errors")]
        public int Errors { get; set; }

        [JsonProperty("timeouts")]
        public int Timeouts { get; set; }

        public void IncrementAttempts()
        {
            this.Attempts += 1;
        }

        public void IncrementErrors()
        {
            this.Errors += 1;
        }

        public void IncrementTimeouts()
        {
            this.Timeouts += 1;
        }

        public Task Reset()
        {
            this.Attempts = 0;
            return Task.CompletedTask;
        }

        public Task<AttemptCounterEntityState> Get()
        {
            var state = new AttemptCounterEntityState()
            {
                AttemptsCount = Attempts,
                ErrorsCount = Errors,
                TimeoutsCount = Timeouts
            };
            return Task.FromResult(state);
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        [FunctionName(nameof(AttemptCounterEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<AttemptCounterEntity>();

    }
}
