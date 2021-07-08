using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        /// <summary>
        /// Delays and returns a status code based on passed in instructions. Post to /api/job with a application/json body similar to this {"returnError": false,"callbackAfterSeconds": 15 }
        /// </summary>
        /// <param name="instructions">Defines how the api should repsond and how much delay there should be before calling back to the durable function.</param>
        /// <returns>Either a 200 or 500 status code with no data.</returns>
        [HttpPost]
        public ActionResult Post(ResponseInstructions instructions)
        {

            // This will run as a background activity and allow the rest of the method to continue.
            // This is the simplest way to do this in this sample, but this is not good practice for real code because .net worker process will recycle and the background task may not complete.
            _ = Task.Run(async () =>
            {
                await Task.Delay(new TimeSpan(0, 0, instructions.CallbackAfterSeconds));
                using var httpClient = new HttpClient();
                _ = await httpClient.PostAsync(instructions.CallbackUri.AbsoluteUri, new StringContent(bool.TrueString));
            });
         
            // Return to caller
            if (instructions.ReturnError)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            else
            {
                return new OkResult();
            }
        }
    }
}
