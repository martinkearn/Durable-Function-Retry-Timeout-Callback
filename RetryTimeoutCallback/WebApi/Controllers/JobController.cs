using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        /// <summary>
        /// Delays and returns a status code based on passed in instructions. Post to /api/job with a application/json body similar to this {"returnError": false,"respondAfterMilliseconds": 1000 }
        /// </summary>
        /// <param name="instructions">Defines how the api should repsond and how much delay there should be.</param>
        /// <returns>Either a 200 or 500 status code with no data.</returns>
        [HttpPost]
        public ActionResult Post(ResponseInstructions instructions)
        {
            if (instructions.RespondAfterMilliseconds > 0)
            {
                Thread.Sleep(instructions.RespondAfterMilliseconds);
            }

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
