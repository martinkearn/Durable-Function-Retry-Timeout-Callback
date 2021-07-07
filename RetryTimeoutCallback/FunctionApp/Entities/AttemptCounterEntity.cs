using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;

namespace FunctionApp.Entities
{
    public class AttemptCounterEntity
    {
        [FunctionName(nameof(AttemptCounterEntity))]
        public static void Counter([EntityTrigger] IDurableEntityContext ctx)
        {
            switch (Enum.Parse(typeof(Enums.AttemptCounterEntityOperation), ctx.OperationName, true))
            {
                case Enums.AttemptCounterEntityOperation.Increment:
                    ctx.SetState(ctx.GetState<int>() + 1);
                    break;
                case Enums.AttemptCounterEntityOperation.Get:
                    ctx.Return(ctx.GetState<int>());
                    break;
            }
        }
    }
}
