using FluentAssertions;
using NUnit.Framework;
using Polly;
using Polly.RateLimiting;
using PollyRateLimiter;

namespace PollyRateLimiter.Tests;

[TestFixture]
public class FixedWindowRateLimiterTests
{
    [Test]
    public void AddFixedWindowRateLimiter_ReturnsBuilder()
    {
        var builder = new ResiliencePipelineBuilder();

        var result = builder.AddFixedWindowRateLimiter(permitLimit: 10, window: TimeSpan.FromSeconds(1));

        result.Should().BeSameAs(builder);
    }

    [Test]
    public void AddFixedWindowRateLimiter_BuildsPipeline()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddFixedWindowRateLimiter(permitLimit: 10, window: TimeSpan.FromSeconds(1))
            .Build();

        pipeline.Should().NotBeNull();
    }

    [Test]
    public async Task AddFixedWindowRateLimiter_PermitsExecutionsUnderLimit()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddFixedWindowRateLimiter(permitLimit: 5, window: TimeSpan.FromSeconds(10))
            .Build();

        var executionCount = 0;
        for (var i = 0; i < 5; i++)
        {
            await pipeline.ExecuteAsync(_ => { executionCount++; return ValueTask.CompletedTask; });
        }

        executionCount.Should().Be(5);
    }

    [Test]
    public async Task AddFixedWindowRateLimiter_RejectsExecutionsOverLimit()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddFixedWindowRateLimiter(permitLimit: 2, window: TimeSpan.FromSeconds(10))
            .Build();

        await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);
        await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);

        var act = async () => await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);

        await act.Should().ThrowAsync<RateLimiterRejectedException>();
    }

    [Test]
    public void AddFixedWindowRateLimiter_InvokesConfigureCallback()
    {
        var callbackInvoked = false;
        var pipeline = new ResiliencePipelineBuilder()
            .AddFixedWindowRateLimiter(
                permitLimit: 10,
                window: TimeSpan.FromSeconds(1),
                configure: _ => callbackInvoked = true)
            .Build();

        callbackInvoked.Should().BeTrue();
    }

    [Test]
    public void AddFixedWindowRateLimiter_NullBuilder_ThrowsArgumentNullException()
    {
        ResiliencePipelineBuilder builder = null!;

        var act = () => builder.AddFixedWindowRateLimiter(permitLimit: 10, window: TimeSpan.FromSeconds(1));

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddFixedWindowRateLimiter_ZeroPermitLimit_ThrowsArgumentOutOfRangeException()
    {
        var builder = new ResiliencePipelineBuilder();

        var act = () => builder.AddFixedWindowRateLimiter(permitLimit: 0, window: TimeSpan.FromSeconds(1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void AddFixedWindowRateLimiter_ZeroWindow_ThrowsArgumentOutOfRangeException()
    {
        var builder = new ResiliencePipelineBuilder();

        var act = () => builder.AddFixedWindowRateLimiter(permitLimit: 10, window: TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void AddFixedWindowRateLimiter_WorksWithGenericBuilder()
    {
        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddFixedWindowRateLimiter(permitLimit: 10, window: TimeSpan.FromSeconds(1))
            .Build();

        pipeline.Should().NotBeNull();
    }
}

[TestFixture]
public class SlidingWindowRateLimiterTests
{
    [Test]
    public void AddSlidingWindowRateLimiter_ReturnsBuilder()
    {
        var builder = new ResiliencePipelineBuilder();

        var result = builder.AddSlidingWindowRateLimiter(permitLimit: 10, window: TimeSpan.FromSeconds(1));

        result.Should().BeSameAs(builder);
    }

    [Test]
    public void AddSlidingWindowRateLimiter_BuildsPipeline()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddSlidingWindowRateLimiter(permitLimit: 10, window: TimeSpan.FromSeconds(1))
            .Build();

        pipeline.Should().NotBeNull();
    }

    [Test]
    public async Task AddSlidingWindowRateLimiter_PermitsExecutionsUnderLimit()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddSlidingWindowRateLimiter(permitLimit: 5, window: TimeSpan.FromSeconds(10))
            .Build();

        var executionCount = 0;
        for (var i = 0; i < 5; i++)
        {
            await pipeline.ExecuteAsync(_ => { executionCount++; return ValueTask.CompletedTask; });
        }

        executionCount.Should().Be(5);
    }

    [Test]
    public async Task AddSlidingWindowRateLimiter_RejectsExecutionsOverLimit()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddSlidingWindowRateLimiter(permitLimit: 2, window: TimeSpan.FromSeconds(10))
            .Build();

        await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);
        await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);

        var act = async () => await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);

        await act.Should().ThrowAsync<RateLimiterRejectedException>();
    }

    [Test]
    public void AddSlidingWindowRateLimiter_CustomSegments_BuildsPipeline()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddSlidingWindowRateLimiter(permitLimit: 10, window: TimeSpan.FromSeconds(1), segmentsPerWindow: 8)
            .Build();

        pipeline.Should().NotBeNull();
    }

    [Test]
    public void AddSlidingWindowRateLimiter_ZeroSegmentsPerWindow_ThrowsArgumentOutOfRangeException()
    {
        var builder = new ResiliencePipelineBuilder();

        var act = () => builder.AddSlidingWindowRateLimiter(
            permitLimit: 10, window: TimeSpan.FromSeconds(1), segmentsPerWindow: 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void AddSlidingWindowRateLimiter_WorksWithGenericBuilder()
    {
        var pipeline = new ResiliencePipelineBuilder<int>()
            .AddSlidingWindowRateLimiter(permitLimit: 10, window: TimeSpan.FromSeconds(1))
            .Build();

        pipeline.Should().NotBeNull();
    }
}

[TestFixture]
public class TokenBucketRateLimiterTests
{
    [Test]
    public void AddTokenBucketRateLimiter_ReturnsBuilder()
    {
        var builder = new ResiliencePipelineBuilder();

        var result = builder.AddTokenBucketRateLimiter(
            tokenLimit: 100, tokensPerPeriod: 10, replenishmentPeriod: TimeSpan.FromSeconds(1));

        result.Should().BeSameAs(builder);
    }

    [Test]
    public void AddTokenBucketRateLimiter_BuildsPipeline()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddTokenBucketRateLimiter(tokenLimit: 100, tokensPerPeriod: 10, replenishmentPeriod: TimeSpan.FromSeconds(1))
            .Build();

        pipeline.Should().NotBeNull();
    }

    [Test]
    public async Task AddTokenBucketRateLimiter_PermitsExecutionsUnderLimit()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddTokenBucketRateLimiter(tokenLimit: 5, tokensPerPeriod: 1, replenishmentPeriod: TimeSpan.FromSeconds(10))
            .Build();

        var executionCount = 0;
        for (var i = 0; i < 5; i++)
        {
            await pipeline.ExecuteAsync(_ => { executionCount++; return ValueTask.CompletedTask; });
        }

        executionCount.Should().Be(5);
    }

    [Test]
    public async Task AddTokenBucketRateLimiter_RejectsExecutionsOverLimit()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddTokenBucketRateLimiter(tokenLimit: 2, tokensPerPeriod: 1, replenishmentPeriod: TimeSpan.FromSeconds(10))
            .Build();

        await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);
        await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);

        var act = async () => await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);

        await act.Should().ThrowAsync<RateLimiterRejectedException>();
    }

    [Test]
    public void AddTokenBucketRateLimiter_ZeroTokenLimit_ThrowsArgumentOutOfRangeException()
    {
        var builder = new ResiliencePipelineBuilder();

        var act = () => builder.AddTokenBucketRateLimiter(
            tokenLimit: 0, tokensPerPeriod: 10, replenishmentPeriod: TimeSpan.FromSeconds(1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void AddTokenBucketRateLimiter_ZeroTokensPerPeriod_ThrowsArgumentOutOfRangeException()
    {
        var builder = new ResiliencePipelineBuilder();

        var act = () => builder.AddTokenBucketRateLimiter(
            tokenLimit: 100, tokensPerPeriod: 0, replenishmentPeriod: TimeSpan.FromSeconds(1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void AddTokenBucketRateLimiter_ZeroReplenishmentPeriod_ThrowsArgumentOutOfRangeException()
    {
        var builder = new ResiliencePipelineBuilder();

        var act = () => builder.AddTokenBucketRateLimiter(
            tokenLimit: 100, tokensPerPeriod: 10, replenishmentPeriod: TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void AddTokenBucketRateLimiter_WorksWithGenericBuilder()
    {
        var pipeline = new ResiliencePipelineBuilder<bool>()
            .AddTokenBucketRateLimiter(tokenLimit: 100, tokensPerPeriod: 10, replenishmentPeriod: TimeSpan.FromSeconds(1))
            .Build();

        pipeline.Should().NotBeNull();
    }

    [Test]
    public async Task AddTokenBucketRateLimiter_OnRejected_IsInvokedOnRejection()
    {
        var rejected = false;
        var pipeline = new ResiliencePipelineBuilder()
            .AddTokenBucketRateLimiter(
                tokenLimit: 1,
                tokensPerPeriod: 1,
                replenishmentPeriod: TimeSpan.FromSeconds(10),
                configure: opts => opts.OnRejected = _ => { rejected = true; return ValueTask.CompletedTask; })
            .Build();

        await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);

        try { await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask); } catch (RateLimiterRejectedException) { }

        rejected.Should().BeTrue();
    }
}
