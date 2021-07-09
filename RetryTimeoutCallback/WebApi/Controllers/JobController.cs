using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading;

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
            // Write callback uri to console
            Debug.WriteLine($"Api has been called");

            // Delay for 5 seconds to simulate a real api.
            Thread.Sleep(3000);

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
