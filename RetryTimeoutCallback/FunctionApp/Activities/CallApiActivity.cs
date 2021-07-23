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
        public async static Task<(HttpStatusCode, string)> CallApi([ActivityTrigger] IDurableActivityContext context)
        {
            // Get input
            var input = context.GetInput<CallApiActivityInput>();

            // Generate random bool indicating whether the api should return sucess or not
            var random = new Random();
            var randomBool = (random.Next(100) < input.ErrorResponseLikelihoodPercentage); 

            // Construct ResponseInstructions
            var instructions = new ResponseInstructions()
            {
                ReturnError = randomBool,
                CallbackUri = input.CallbackUri,
            };

            // Make request to api
            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.PostAsJsonAsync("http://localhost:5000/api/Job", instructions);

            // Prepare tuple to return
            var response = (StatusCode: responseMessage.StatusCode, StatusText: responseMessage.ReasonPhrase);

            // Return
            return response;
        }
    }
}
