using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace TaskFlow.Processor.Resilience;
public static class ResiliencePipelineFactory
{
    public static ResiliencePipeline Create(ILogger logger)
    {
        return new ResiliencePipelineBuilder()

            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Publish retry {Attempt}/3 — {Exception}",
                        args.AttemptNumber + 1,
                        args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })

            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30),

                OnOpened = args =>
                {
                    logger.LogCritical(
                        "Circuit breaker OPENED — publishing halted for 30s. Reason: {Exception}",
                        args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    logger.LogInformation("Circuit breaker CLOSED — publishing resumed.");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    logger.LogInformation("Circuit breaker HALF-OPEN — testing broker connectivity.");
                    return ValueTask.CompletedTask;
                }
            })

            .Build();
    }
}