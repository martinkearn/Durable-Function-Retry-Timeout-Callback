using FunctionApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;

namespace FunctionApp.Entities
{
    public class RetryCounterEntity
    {
        [FunctionName(nameof(RetryCounterEntity))]
        public static void Counter([EntityTrigger] IDurableEntityContext ctx)
        {
            switch (Enum.Parse(typeof(Enums.RetryCounterEntityOperation), ctx.OperationName, true))
            {
                case Enums.RetryCounterEntityOperation.Increment:
                    ctx.SetState(ctx.GetState<int>() + 1);
                    break;
                case Enums.RetryCounterEntityOperation.Get:
                    ctx.Return(ctx.GetState<int>());
                    break;
            }
        }
    }
}
