using FunctionApp.Entities;
using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp.Clients
{
    public class HttpAttemptCounterEntityClient
    {
        /// <summary>
        /// Returns the latest commited entity state for the given instanceid. Expect GET request /api/HttpAttemptCounterEntityClient?instanceid=[instanceidhere]
        /// </summary>
        /// <param name="req">HttpRequestMessage.</param>
        /// <param name="client">IDurableEntityClient.</param>
        /// <param name="log">ILogger.</param>
        /// <returns>The latest commmited AttemptCounterEntityState for the given orchestration instance id.</returns>
        [FunctionName(nameof(HttpAttemptCounterEntityClient))]
        public static async Task<AttemptCounterEntityState> HttpAttemptCounterEntity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            // Get insanceId from input payload
            var queryString = req.RequestUri.ParseQueryString();
            var instanceId = queryString["instanceid"];

            // Setup entity ID key'd from the orchestration instance id. One counter per orchestration instance.
            var attemptCounterEntityId = new EntityId(nameof(AttemptCounterEntity), instanceId);

            // get entity state
            var stateResponse = await client.ReadEntityStateAsync<AttemptCounterEntityState>(attemptCounterEntityId);

            // Return
            return stateResponse.EntityState;
        }
    }
}
