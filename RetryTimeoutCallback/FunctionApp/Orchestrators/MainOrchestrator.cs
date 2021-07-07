using System.Collections.Generic;
using System.Threading.Tasks;
using FunctionApp.Activities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace FunctionApp.Orchestrators
{
    public static class MainOrchestrator
    {
        [FunctionName(nameof(MainOrchestrator))]
        public static async Task<List<string>> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>
            {
                // Replace "hello" with the name of your Durable Activity Function.
                await context.CallActivityAsync<string>(nameof(SayHelloActivity), "Tokyo"),
                await context.CallActivityAsync<string>(nameof(SayHelloActivity), "Seattle"),
                await context.CallActivityAsync<string>(nameof(SayHelloActivity), "London")
            };

            return outputs;
        }
    }
}