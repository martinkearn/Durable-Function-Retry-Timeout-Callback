namespace FunctionApp.Models
{
    public static class Constants
    {
        /// <summary>
        /// A fake instance id used to generate management payload data which can be swapped later for a real instance id.
        /// </summary>
        public const string TempInstanceId = "551c64b1-d7d4-4b3a-a2cf-f33697d10105";

        /// <summary>
        /// Name of the event that the external service will use to call back to the orchestration.
        /// </summary>
        public const string CallbackEventName = "Callback";

        /// <summary>
        /// Token used in the management payload data to be swapped by real event name.
        /// </summary>
        public const string SendEventPostUriEventNameToken = "{eventName}";
    }
}
