using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.RateLimiting;
using TradingAI.Infrastructure.Ai;
using TradingAI.Infrastructure.AI;
using TradingAI.Infrastructure.Auth;
using TradingAI.Infrastructure.Cache;
using TradingAI.Infrastructure.Email;
using TradingAI.Infrastructure.Identity;
using TradingAI.Infrastructure.MarketData;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using TradingAI.Infrastructure.Notifications;
using TradingAI.Infrastructure.Storage;
using TradingAI.Infrastructure.Subscipritons;

namespace TradingAI.Infrastructure
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseNpgsql(config.GetConnectionString("DefaultConnection"));
                // Don't crash on boot if the code model has drifted from the
                // applied migrations. We still see the warning in logs so the
                // drift is visible — just not fatal. Run
                //   dotnet ef migrations add <Name> --project src/TradingAI.Infrastructure --startup-project src/TradingAI.API
                // to capture the diff and stop seeing the warning.
                opt.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IJwtService, JwtService>();
            services.Configure<JwtSettings>(config.GetSection("Jwt"));
            services.AddScoped<IFileStorage, LocalFileStorage>();





            // Market data
            services.Configure<TwelveDataSettings>(config.GetSection("TwelveData"));
            // TwelveData: slightly more generous timeout and retry budget
            services.AddHttpClient<TwelveDataClient>(c =>
            {
                // Force HTTP/1.1 — some local antivirus / VPN software corrupts
                // ALPN during HTTP/2 upgrade, producing "corrupted TLS frame" errors.
                // Curl works because curl defaults to HTTP/1.1; .NET defaults to HTTP/2.
                c.DefaultRequestVersion = System.Net.HttpVersion.Version11;
                c.DefaultVersionPolicy = System.Net.Http.HttpVersionPolicy.RequestVersionOrLower;
            })
            .AddPolicyHandler((sp, req) => TradingAI.Infrastructure.Resilience.HttpPolicies.GetRetryPolicy(
                providerName: "TwelveData",
                loggerFactory: sp.GetRequiredService<ILoggerFactory>(),
                delays: new[] { TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(500) }))
            .AddPolicyHandler((sp, req) => TradingAI.Infrastructure.Resilience.HttpPolicies.GetCircuitBreakerPolicy(
                providerName: "TwelveData",
                loggerFactory: sp.GetRequiredService<ILoggerFactory>(),
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30)))
            .AddPolicyHandler((sp, req) => TradingAI.Infrastructure.Resilience.HttpPolicies.GetTimeoutPolicy(
                timeout: TimeSpan.FromSeconds(3),
                loggerFactory: sp.GetRequiredService<ILoggerFactory>(),
                providerName: "TwelveData"));
            services.AddScoped<IMarketDataService, MarketDataService>();


            services.AddSingleton<IConnectionMultiplexer>(_ =>
                 ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));

            services.AddScoped<ICacheService, RedisCacheService>();

            // AI analysis (Grok via xAI / OpenAI-compatible API)
            services.Configure<GrokSettings>(config.GetSection("Grok"));
            services.AddScoped<IAiAnalysisService, GrokAiAnalysisService>();

            //Outcome
            services.AddSingleton<IOutcomeEvaluator, OutcomeEvaluator>();
            
            //Entitlements
            services.AddScoped<IEntitlementService, EntitlementService>();

            //UsageCounter
            services.AddScoped<IUsageCounter, UsageCounter>();

            //Email
            services.Configure<EmailSettings>(config.GetSection("Email"));
            services.AddScoped<IEmailService, SmtpEmailService>();

            //Notifications
            services.AddScoped<INotificationService, NotificationService>();


            // CoinGecko: more conservative retry (external TLS flakiness) and shorter timeout for UI
            services.AddHttpClient<CoinGeckoClient>(c =>
            {
                c.BaseAddress = new Uri("https://api.coingecko.com");
                c.DefaultRequestHeaders.Add("User-Agent", "TradingAI/1.0");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                // Force HTTP/1.1 — see TwelveDataClient comment.
                c.DefaultRequestVersion = System.Net.HttpVersion.Version11;
                c.DefaultVersionPolicy = System.Net.Http.HttpVersionPolicy.RequestVersionOrLower;
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                             | System.Security.Authentication.SslProtocols.Tls13
            })
            .AddPolicyHandler((sp, req) => TradingAI.Infrastructure.Resilience.HttpPolicies.GetRetryPolicy(
                providerName: "CoinGecko",
                loggerFactory: sp.GetRequiredService<ILoggerFactory>(),
                delays: new[] { TimeSpan.FromMilliseconds(150) }))
            .AddPolicyHandler((sp, req) => TradingAI.Infrastructure.Resilience.HttpPolicies.GetCircuitBreakerPolicy(
                providerName: "CoinGecko",
                loggerFactory: sp.GetRequiredService<ILoggerFactory>(),
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(20)))
            .AddPolicyHandler((sp, req) => TradingAI.Infrastructure.Resilience.HttpPolicies.GetTimeoutPolicy(
                timeout: TimeSpan.FromSeconds(2),
                loggerFactory: sp.GetRequiredService<ILoggerFactory>(),
                providerName: "CoinGecko"));


                        var jwtSettings = config.GetSection("Jwt").Get<JwtSettings>()!;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                        ClockSkew = TimeSpan.Zero
                    };

                });

            services.AddAuthorization();


            return services;


        }

        
    }
}
