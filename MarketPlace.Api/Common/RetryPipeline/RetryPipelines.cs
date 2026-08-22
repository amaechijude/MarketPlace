using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace MarketPlace.Api.Common.RetryPipeline;

public static class RetryPipelines
{
    public static ResiliencePipeline DbConcurrency =>
        new ResiliencePipelineBuilder()
            .AddRetry(
                new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<DbUpdateConcurrencyException>(),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                }
            )
            .Build();
}
