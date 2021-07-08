using System;

namespace Domain
{
    public class ResponseInstructions
    {
        /// <summary>
        /// If true, the api will return a 500 status code, if false it will return a 200 status code.
        /// </summary>
        public bool ReturnError { get; set; }

        /// <summary>
        /// Defines how long in seconds the API should wait before calling back.
        /// </summary>
        public int CallbackAfterSeconds { get; set; }

        /// <summary>
        /// The uri the Api should call back to. This should rehydrate the durable function using the WaitForExternalEvent method.
        /// </summary>
        public Uri CallbackUri { get; set; }
    }
}
