using Polly;
using Polly.RateLimiting;
using System.Threading.RateLimiting;

namespace PollyRateLimiter;

/// <summary>
/// Extension methods on <see cref="ResiliencePipelineBuilderBase"/> for common
/// <see cref="System.Threading.RateLimiting"/> strategies not built into Polly:
/// fixed-window, sliding-window, and token-bucket.
/// </summary>
public static class RateLimiterBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="FixedWindowRateLimiter"/> strategy to the resilience pipeline.
    /// </summary>
    /// <param name="builder">The resilience pipeline builder.</param>
    /// <param name="permitLimit">Maximum number of permits per window.</param>
    /// <param name="window">Duration of each window.</param>
    /// <param name="queueLimit">
    /// Number of executions to queue when the limit is reached.
    /// Defaults to <c>0</c> (reject immediately).
    /// </param>
    /// <param name="configure">Optional callback to further configure <see cref="RateLimiterStrategyOptions"/>.</param>
    public static TBuilder AddFixedWindowRateLimiter<TBuilder>(
        this TBuilder builder,
        int permitLimit,
        TimeSpan window,
        int queueLimit = 0,
        Action<RateLimiterStrategyOptions>? configure = null)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(permitLimit);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(window, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfNegative(queueLimit);

        var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = queueLimit,
            AutoReplenishment = true
        });

        return builder.AddRateLimiter(BuildOptions(limiter, configure));
    }

    /// <summary>
    /// Adds a <see cref="SlidingWindowRateLimiter"/> strategy to the resilience pipeline.
    /// </summary>
    /// <param name="builder">The resilience pipeline builder.</param>
    /// <param name="permitLimit">Maximum number of permits per window.</param>
    /// <param name="window">Duration of the sliding window.</param>
    /// <param name="segmentsPerWindow">
    /// Number of segments to divide the window into. Defaults to <c>4</c>.
    /// Higher values give smoother rate limiting.
    /// </param>
    /// <param name="queueLimit">
    /// Number of executions to queue when the limit is reached.
    /// Defaults to <c>0</c> (reject immediately).
    /// </param>
    /// <param name="configure">Optional callback to further configure <see cref="RateLimiterStrategyOptions"/>.</param>
    public static TBuilder AddSlidingWindowRateLimiter<TBuilder>(
        this TBuilder builder,
        int permitLimit,
        TimeSpan window,
        int segmentsPerWindow = 4,
        int queueLimit = 0,
        Action<RateLimiterStrategyOptions>? configure = null)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(permitLimit);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(window, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(segmentsPerWindow);
        ArgumentOutOfRangeException.ThrowIfNegative(queueLimit);

        var limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            SegmentsPerWindow = segmentsPerWindow,
            QueueLimit = queueLimit,
            AutoReplenishment = true
        });

        return builder.AddRateLimiter(BuildOptions(limiter, configure));
    }

    /// <summary>
    /// Adds a <see cref="TokenBucketRateLimiter"/> strategy to the resilience pipeline.
    /// </summary>
    /// <param name="builder">The resilience pipeline builder.</param>
    /// <param name="tokenLimit">Maximum number of tokens (burst capacity).</param>
    /// <param name="tokensPerPeriod">Number of tokens replenished each period.</param>
    /// <param name="replenishmentPeriod">How often tokens are replenished.</param>
    /// <param name="queueLimit">
    /// Number of executions to queue when the limit is reached.
    /// Defaults to <c>0</c> (reject immediately).
    /// </param>
    /// <param name="configure">Optional callback to further configure <see cref="RateLimiterStrategyOptions"/>.</param>
    public static TBuilder AddTokenBucketRateLimiter<TBuilder>(
        this TBuilder builder,
        int tokenLimit,
        int tokensPerPeriod,
        TimeSpan replenishmentPeriod,
        int queueLimit = 0,
        Action<RateLimiterStrategyOptions>? configure = null)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokenLimit);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokensPerPeriod);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(replenishmentPeriod, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfNegative(queueLimit);

        var limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = tokenLimit,
            TokensPerPeriod = tokensPerPeriod,
            ReplenishmentPeriod = replenishmentPeriod,
            QueueLimit = queueLimit,
            AutoReplenishment = true
        });

        return builder.AddRateLimiter(BuildOptions(limiter, configure));
    }

    private static RateLimiterStrategyOptions BuildOptions(RateLimiter limiter, Action<RateLimiterStrategyOptions>? configure)
    {
        var opts = new RateLimiterStrategyOptions
        {
            RateLimiter = args => limiter.AcquireAsync(1, args.Context.CancellationToken)
        };
        configure?.Invoke(opts);
        return opts;
    }
}
