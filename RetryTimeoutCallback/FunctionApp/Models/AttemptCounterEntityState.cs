using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FunctionApp.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class AttemptCounterEntityState
    {
        [JsonProperty("attempts")]
        public List<Attempt> Attempts { get; set; } = new List<Attempt>();

        [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
        public struct Attempt
        {
            public int Order;
            public DateTime DateTimeStarted;
            public string State;
            public string StatusText;
        }
    }
}
