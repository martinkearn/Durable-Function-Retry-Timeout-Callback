using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FunctionApp.Models.AttemptCounterEntityState;

namespace FunctionApp.Entities
{
    public class AttemptCounterEntity
    {
        [JsonProperty("attempts")]
        public List<Attempt> Attempts { get; set; }

        public void AddAttempt(Attempt newAttempt)
        {
            if (this.Attempts == default)
            {
                this.Attempts = new List<Attempt>();
            }
            newAttempt.Order = this.Attempts.Count + 1;
            this.Attempts.Add(newAttempt);
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
                Attempts = this.Attempts
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
