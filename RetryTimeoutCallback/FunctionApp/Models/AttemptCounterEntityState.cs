using Newtonsoft.Json;

namespace FunctionApp.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class AttemptCounterEntityState
    {
        [JsonProperty("attemptsCount")]
        public int AttemptsCount { get; set; }

        [JsonProperty("timeoutsCount")]
        public int TimeoutsCount { get; set; }

        [JsonProperty("errorsCount")]
        public int ErrorsCount { get; set; }
    }
}
