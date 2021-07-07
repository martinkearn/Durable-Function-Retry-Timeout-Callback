using System.Net;
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
        [FunctionName(nameof(MainOrchestrator))]
        public static async Task<string> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            // Setup entity ID key'd from the orchestration instance id. One counter per orchestration instance.
            var attemptCounterEntityId = new EntityId(nameof(AttemptCounterEntity), context.InstanceId);

            // Call Api until we get success or have reached the retry limit
            var attemptLimit = 5;
            var attempts = 0;
            HttpStatusCode status;
            do
            {
                // Increment attempt counter
                context.SignalEntity(attemptCounterEntityId, Enums.AttemptCounterEntityOperation.Increment.ToString());

                // Do work here
                status = await context.CallActivityAsync<HttpStatusCode>(nameof(CallApiActivity), null);

                // Get current counter value
                attempts = await context.CallEntityAsync<int>(attemptCounterEntityId, Enums.AttemptCounterEntityOperation.Get.ToString());
            } 
            while
            (
                (attempts < attemptLimit)
                &&
                (status != HttpStatusCode.OK)
            );

            return $"Finished with status of {status} after {attempts} attempts";
        }
    }
}