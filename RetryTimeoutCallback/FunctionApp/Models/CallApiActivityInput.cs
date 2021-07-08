using System;

namespace FunctionApp.Models
{
    public class CallApiActivityInput
    {
        /// <summary>
        /// The url the Api will call back to.
        /// </summary>
        public Uri CallbackUri { get; set; }

        /// <summary>
        /// The Api will call back to the durable function after this many seconds.
        /// </summary>
        public int CallbackAfterSeconds { get; set; }

        /// <summary>
        /// Api will return an error this % of the time. Use 0 if you never want an error, 100 if you always want an error.
        /// </summary>
        public int ErrorResponseLikelihoodPercentage { get; set; }
    }
}
