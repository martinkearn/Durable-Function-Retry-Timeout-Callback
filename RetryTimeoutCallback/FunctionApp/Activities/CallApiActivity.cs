using Domain;
using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp.Activities
{
    public static class CallApiActivity
    {
        [FunctionName(nameof(CallApiActivity))]
        public async static Task<HttpStatusCode> CallApi([ActivityTrigger] IDurableActivityContext context)
        {
            // Get input
            var input = context.GetInput<CallApiActivityInput>();

            // Generate random bool indicating whether the api should return sucess or not
            var random = new Random();
            var randomBool = (random.Next(100) < input.ErrorResponseLikelihoodPercentage); 

            // Construct ResponseInstructions
            var instructions = new ResponseInstructions()
            {
                CallbackAfterSeconds = input.CallbackAfterSeconds,
                ReturnError = randomBool,
                CallbackUri = input.CallbackUri,
            };

            // Make request to api
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync("http://localhost:5000/api/Job", instructions);
            
            // Return status code
            return response.StatusCode;
        }
    }
}
