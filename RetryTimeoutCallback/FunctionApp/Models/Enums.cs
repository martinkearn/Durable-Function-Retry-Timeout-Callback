using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FunctionApp.Models
{
    public static class Enums
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum AttemptState
        {
            /// <summary>
            /// Used for when an Attempt is newly created and has not undertaken any work yet
            /// </summary>
            New,

            /// <summary>
            /// Used for when an attempt is being executed (prepare, execute, cleanup)
            /// </summary>
            Executing,

            /// <summary>
            /// Used when an attempt has finished executing with success
            /// </summary>
            ExecutedSuccess,

            /// <summary>
            /// Used when an attempt has finished executing with failure
            /// </summary>
            ExecutedFailed,

            /// <summary>
            /// Used when an attempt is waiting for callback from the compute service
            /// </summary>
            WaitingForCallback,

            /// <summary>
            /// Used when an attempt has received a sucessfull callback from the compute service
            /// </summary>
            CallbackSuccess,

            /// <summary>
            /// Used when an attempt has received a failure callback from the compute service
            /// </summary>
            CallbackFailure,

            /// <summary>
            /// Used when an attempt has not received a callback from the compute service within the defined timeout period
            /// </summary>
            TimedOut,
        }
    }
}
