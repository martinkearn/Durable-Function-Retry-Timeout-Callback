using System.Threading.Tasks;
using FunctionApp.Entities;
using FunctionApp.Models;
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
            var retryAttempts = 0;
            do
            {
                // Do work here

                // Incremenet counter
                context.SignalEntity(retryCounterEntityId, Enums.RetryCounterEntityOperation.Increment.ToString());

                // get current counter value
                retryAttempts = await context.CallEntityAsync<int>(retryCounterEntityId, Enums.RetryCounterEntityOperation.Get.ToString());
            } while (retryAttempts < retryLimit);

            // Get final counter value.
            var finalRetryCounterValue = await context.CallEntityAsync<int>(retryCounterEntityId, Enums.RetryCounterEntityOperation.Get.ToString());

            return $"Have attempted {finalRetryCounterValue} retrys";
        }
    }
}