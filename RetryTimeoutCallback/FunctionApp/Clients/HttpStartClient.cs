using FunctionApp.Models;
using FunctionApp.Orchestrators;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp.Clients
{
    public class HttpStartClient
    {
        [FunctionName(nameof(HttpStartClient))]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Create callback api and input payload
            var managementPayload = starter.CreateHttpManagementPayload(Constants.TempInstanceId);
            var mainOrchestrationInput = new MainOrchestrationInput() { SendEventPostUri = new Uri(managementPayload.SendEventPostUri) };

            // Start orchestrator
            string instanceId = await starter.StartNewAsync(nameof(MainOrchestrator), mainOrchestrationInput);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
