using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace FunctionApp.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class AttemptCounterEntityState
    {
        [JsonProperty("attempts")]
        public List<Attempt> Attempts { get; set; } = new List<Attempt>();

        [JsonProperty("overallstate")]
        public string OverallState { get; set; } = "new";

        [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
        public struct Attempt
        {
            public Guid Id;
            public DateTime DateTimeStarted;
            public DateTime DateTimeStateSet;
            public string State;
            public string StatusText;
            public HttpStatusCode? StatusCode;
        }
    }
}
