namespace Domain
{
    public class ResponseInstructions
    {
        /// <summary>
        /// If true, the api will return a 500 status code, if false it will return a 200 status code.
        /// </summary>
        public bool ReturnError { get; set; }

        /// <summary>
        /// Defines how long in milliseconds the API should wait before responding.
        /// </summary>
        public int RespondAfterMilliseconds { get; set; }
    }
}
