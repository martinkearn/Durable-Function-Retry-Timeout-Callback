using System.Threading.Tasks;
using FunctionApp.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace FunctionApp.Orchestrators
{
    public static class MainOrchestrator
    {
        [FunctionName(nameof(MainOrchestrator))]
        public static async Task<string> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var retryLimit = 3;

            // Setup entity ID key'd from the orchestration instance id. One counter per orchestration instance.
            var retryCounterEntityId = new EntityId(nameof(RetryCounterEntity), context.InstanceId);

            // Increment counter until it is equal to the retryLimit
            do
            {
                // do work here
                context.SignalEntity(retryCounterEntityId, "increment");
            } while (await context.CallEntityAsync<int>(retryCounterEntityId, "get") < retryLimit);

            // Get final counter value.
            var finalRetryCounterValue = await context.CallEntityAsync<int>(retryCounterEntityId, "get");

            return $"Final count {finalRetryCounterValue}";
        }
    }
}