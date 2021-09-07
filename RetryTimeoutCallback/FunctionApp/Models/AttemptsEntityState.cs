using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static FunctionApp.Models.Enums;

namespace FunctionApp.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class AttemptsEntityState
    {
        [JsonProperty("attempts")]
        public Dictionary<Guid, Attempt> Attempts { get; set; } = new Dictionary<Guid, Attempt>();

        [JsonProperty("overallstate")]
        public string OverallState { get; set; } = "new";

        [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
        public struct Attempt
        {
            public Guid Id;
            public DateTime Started;
            public DateTime StateSet;
            public DateTime? TimeoutDue;
            public AttemptState State;
            public string Message;
            public bool IsSuccess;
        }
    }
}
