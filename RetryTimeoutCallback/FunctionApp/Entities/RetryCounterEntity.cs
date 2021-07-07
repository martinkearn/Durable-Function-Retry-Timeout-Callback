using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace FunctionApp.Entities
{
    public class RetryCounterEntity
    {
        [FunctionName(nameof(RetryCounterEntity))]
        public static void Counter([EntityTrigger] IDurableEntityContext ctx)
        {
            switch (ctx.OperationName.ToLowerInvariant())
            {
                case "increment":
                    ctx.SetState(ctx.GetState<int>() + 1);
                    break;
                case "get":
                    ctx.Return(ctx.GetState<int>());
                    break;
            }
        }
    }
}
