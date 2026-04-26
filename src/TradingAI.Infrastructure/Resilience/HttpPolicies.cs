using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Net.Http;

namespace TradingAI.Infrastructure.Resilience;

public static class HttpPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(string providerName, ILoggerFactory loggerFactory, TimeSpan[] delays, HttpRequestMessage? request = null)
    {
        var logger = loggerFactory.CreateLogger("HttpPolicies");
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(delays, onRetry: (outcome, timespan, retryCount, ctx) =>
            {
                string? uri = null;
                if (request?.RequestUri is not null) uri = request.RequestUri.ToString();
                else if (ctx.TryGetValue("requestUri", out var v) && v is string s) uri = s;
                using (logger.BeginScope(new Dictionary<string, object?> { ["Provider"] = providerName, ["RequestUri"] = uri }))
                {
                    logger.LogWarning("Retry {RetryCount} after {Delay} due to {Reason}", retryCount, timespan, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                }
            });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(string providerName, ILoggerFactory loggerFactory, int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak, HttpRequestMessage? request = null)
    {
        var logger = loggerFactory.CreateLogger("HttpPolicies");
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking,
                durationOfBreak,
                onBreak: (outcome, ts) =>
                {
                    var uri = request?.RequestUri?.ToString();
                    using (logger.BeginScope(new Dictionary<string, object?> { ["Provider"] = providerName, ["RequestUri"] = uri }))
                    {
                        logger.LogWarning("Circuit broken for {Duration} due to {Reason}", ts, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    }
                },
                onReset: () => logger.LogInformation("Circuit for {Provider} reset", providerName),
                onHalfOpen: () => logger.LogInformation("Circuit for {Provider} is half-open", providerName));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout, ILoggerFactory loggerFactory, string providerName, HttpRequestMessage? request = null)
    {
        var logger = loggerFactory.CreateLogger("HttpPolicies");
        var policy = Policy.TimeoutAsync<HttpResponseMessage>(timeout, TimeoutStrategy.Optimistic)
            .WithPolicyKey($"Timeout-{providerName}");
        return policy;
    }
}
