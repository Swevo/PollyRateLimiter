# PollyRateLimiter

[![NuGet](https://img.shields.io/nuget/v/PollyRateLimiter
[![NuGet Downloads](https://img.shields.io/nuget/dt/PollyRateLimiter.svg)](https://www.nuget.org/packages/PollyRateLimiter).svg)](https://www.nuget.org/packages/PollyRateLimiter/)
[![Build](https://github.com/Swevo/PollyRateLimiter/actions/workflows/build.yml/badge.svg)](https://github.com/Swevo/PollyRateLimiter/actions/workflows/build.yml)

Convenience extension methods for **Polly v8** resilience pipelines.

Polly v8 ships with `AddConcurrencyLimiter` out of the box, but leaves `FixedWindow`, `SlidingWindow`, and `TokenBucket` as bring-your-own. This package fills that gap with one-liner extension methods for all three strategies on top of `System.Threading.RateLimiting`.

## Install

```
dotnet add package PollyRateLimiter
```

## Usage

```csharp
using PollyRateLimiter;

// Fixed window: 100 requests per second
var pipeline = new ResiliencePipelineBuilder()
    .AddFixedWindowRateLimiter(permitLimit: 100, window: TimeSpan.FromSeconds(1))
    .Build();

// Sliding window: 100 requests per second, smoothed into 4 segments
var pipeline = new ResiliencePipelineBuilder()
    .AddSlidingWindowRateLimiter(permitLimit: 100, window: TimeSpan.FromSeconds(1), segmentsPerWindow: 4)
    .Build();

// Token bucket: burst up to 200, replenish 50 per second
var pipeline = new ResiliencePipelineBuilder()
    .AddTokenBucketRateLimiter(tokenLimit: 200, tokensPerPeriod: 50, replenishmentPeriod: TimeSpan.FromSeconds(1))
    .Build();
```

### Queue limit

By default, executions that exceed the limit are rejected immediately (`queueLimit: 0`). Pass a positive value to queue them instead:

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddFixedWindowRateLimiter(permitLimit: 10, window: TimeSpan.FromSeconds(1), queueLimit: 5)
    .Build();
```

### OnRejected callback

Use the optional `configure` callback to attach an `OnRejected` handler or change other `RateLimiterStrategyOptions`:

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddTokenBucketRateLimiter(
        tokenLimit: 100,
        tokensPerPeriod: 20,
        replenishmentPeriod: TimeSpan.FromSeconds(1),
        configure: opts => opts.OnRejected = args =>
        {
            logger.LogWarning("Rate limit exceeded. RetryAfter: {RetryAfter}", args.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry) ? retry : TimeSpan.Zero);
            return ValueTask.CompletedTask;
        })
    .Build();
```

### Generic builder

All methods work with `ResiliencePipelineBuilder<T>` too:

```csharp
var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddFixedWindowRateLimiter(permitLimit: 50, window: TimeSpan.FromSeconds(1))
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>())
    .Build();
```

## Comparison with Polly built-ins

| Strategy | Polly built-in | PollyRateLimiter |
|---|---|---|
| Concurrency | `AddConcurrencyLimiter(maxConcurrency)` | — |
| Fixed window | ❌ | `AddFixedWindowRateLimiter(permits, window)` |
| Sliding window | ❌ | `AddSlidingWindowRateLimiter(permits, window)` |
| Token bucket | ❌ | `AddTokenBucketRateLimiter(capacity, perPeriod, period)` |

## Support

If PollyRateLimiter simplifies your rate limiting setup, consider supporting the project:

[![Sponsor](https://img.shields.io/badge/Sponsor-%E2%9D%A4-pink?logo=github)](https://github.com/sponsors/Swevo)

> 💼 **Need .NET resilience help?** Visit [solidqualitysolutions.com](https://solidqualitysolutions.com/) for consulting and architecture services.

| [PollyMailKit](https://github.com/Swevo/PollyMailKit) | MailKit SMTP email client |
| [PollyAzureQueueStorage](https://github.com/Swevo/PollyAzureQueueStorage) | Azure Queue Storage QueueClient |
| [PollyHangfire](https://github.com/Swevo/PollyHangfire) | Hangfire IBackgroundJobClient |

## Also by the same author

> 🌐 **[swevo.github.io](https://swevo.github.io/)**

| Package | Description |
|---|---|
| [**AutoLog.Generator**](https://github.com/Swevo/AutoLog.Generator) | Compile-time high-performance logging — `[Log(Level, Message)]` generates `LoggerMessage.Define`. AOT-safe. |
| [**AutoHttpClient.Generator**](https://github.com/Swevo/AutoHttpClient.Generator) | Compile-time typed HTTP client — `[HttpClient]` on an interface generates a strongly-typed client. AOT-safe Refit alternative. |
| [**AutoDispatch.Generator**](https://github.com/Swevo/AutoDispatch.Generator) | Compile-time CQRS dispatcher — `[Handler]` generates a strongly-typed `IDispatcher`. MediatR alternative. |
| [**AutoWire**](https://github.com/Swevo/AutoWire) | Compile-time DI auto-registration — `[Scoped]`/`[Singleton]`/`[Transient]` generates `IServiceCollection` registration code. |
| [**AutoMap.Generator**](https://github.com/Swevo/AutoMap.Generator) | Compile-time object mapping — `[Map(typeof(Dto))]` generates `ToDto()` extension methods. AutoMapper alternative. |
## License

MIT
