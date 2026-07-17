# PollyRateLimiter

[![NuGet](https://img.shields.io/nuget/v/PollyRateLimiter.svg)](https://www.nuget.org/packages/PollyRateLimiter)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PollyRateLimiter.svg)](https://www.nuget.org/packages/PollyRateLimiter)
[![Build](https://github.com/Swevo/PollyRateLimiter/actions/workflows/build.yml/badge.svg)](https://github.com/Swevo/PollyRateLimiter/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

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

## Related Packages

| Package | Downloads | Description |
|---|---|---|
| [PollyHealthChecks](https://www.nuget.org/packages/PollyHealthChecks) | [![Downloads](https://img.shields.io/nuget/dt/PollyHealthChecks.svg)](https://www.nuget.org/packages/PollyHealthChecks) | ASP.NET Core health checks for Polly v8 circuit breakers — expose circuit-breaker state (Closed, HalfOpen, Open, Isolated) as /health endpoint responses |
| [PollyOpenTelemetry](https://www.nuget.org/packages/PollyOpenTelemetry) | [![Downloads](https://img.shields.io/nuget/dt/PollyOpenTelemetry.svg)](https://www.nuget.org/packages/PollyOpenTelemetry) | OpenTelemetry instrumentation for Polly v8 resilience pipelines |
| [PollyBackoff](https://www.nuget.org/packages/PollyBackoff) | [![Downloads](https://img.shields.io/nuget/dt/PollyBackoff.svg)](https://www.nuget.org/packages/PollyBackoff) | Backoff delay strategies for Polly v8 resilience pipelines |
| [PollyEFCore](https://www.nuget.org/packages/PollyEFCore) | [![Downloads](https://img.shields.io/nuget/dt/PollyEFCore.svg)](https://www.nuget.org/packages/PollyEFCore) | Polly v8 resilience pipelines for Entity Framework Core — wrap every EF Core query and SaveChanges with retry, timeout and circuit-breaker via a single AddPollyResilience() call |
| [PollyRabbitMQ](https://www.nuget.org/packages/PollyRabbitMQ) | [![Downloads](https://img.shields.io/nuget/dt/PollyRabbitMQ.svg)](https://www.nuget.org/packages/PollyRabbitMQ) | Polly v8 resilience for RabbitMQ.Client v7+ — retry, circuit-breaker, and timeout for IChannel operations, with built-in RabbitMqTransientErrors predicate covering AlreadyClosedException, BrokerUnreachableException, OperationInterruptedException, and ConnectFailureException |
| [PollyMongo](https://www.nuget.org/packages/PollyMongo) | [![Downloads](https://img.shields.io/nuget/dt/PollyMongo.svg)](https://www.nuget.org/packages/PollyMongo) | Polly v8 resilience pipelines for MongoDB.Driver — wrap Find, InsertOne, UpdateOne, DeleteOne and other IMongoCollection calls with retry, timeout, circuit-breaker, and more using a single ResilientMongoCollection decorator |
| [PollyDapper](https://www.nuget.org/packages/PollyDapper) | [![Downloads](https://img.shields.io/nuget/dt/PollyDapper.svg)](https://www.nuget.org/packages/PollyDapper) | Polly v8 resilience pipelines for Dapper — wrap QueryAsync, ExecuteAsync, and other Dapper calls with retry, timeout, circuit-breaker, and more using a single ResilientDbConnection decorator |
| [PollyMediatR](https://www.nuget.org/packages/PollyMediatR) | [![Downloads](https://img.shields.io/nuget/dt/PollyMediatR.svg)](https://www.nuget.org/packages/PollyMediatR) | Polly v8 resilience pipelines for MediatR — add retry, timeout, circuit-breaker, rate-limiting, hedging, and chaos engineering to any MediatR request handler with a single line of DI registration |
| [PollySqlClient](https://www.nuget.org/packages/PollySqlClient) | [![Downloads](https://img.shields.io/nuget/dt/PollySqlClient.svg)](https://www.nuget.org/packages/PollySqlClient) | Polly v8 resilience pipelines for Microsoft.Data.SqlClient (SQL Server and Azure SQL) — retry, timeout, and circuit-breaker for SqlConnection queries and commands, plus a built-in SqlServerTransientErrors predicate covering all common SQL Server and Azure SQL transient error numbers |
| [PollyRedis](https://www.nuget.org/packages/PollyRedis) | [![Downloads](https://img.shields.io/nuget/dt/PollyRedis.svg)](https://www.nuget.org/packages/PollyRedis) | Polly v8 resilience for StackExchange.Redis |

## License

MIT
