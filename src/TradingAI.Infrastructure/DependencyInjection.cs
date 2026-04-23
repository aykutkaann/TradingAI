using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Infrastructure.Ai;
using TradingAI.Infrastructure.Auth;
using TradingAI.Infrastructure.Cache;
using TradingAI.Infrastructure.Identity;
using TradingAI.Infrastructure.MarketData;

namespace TradingAI.Infrastructure
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(config.GetConnectionString("DefaultConnection")));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IJwtService, JwtService>();
            services.Configure<JwtSettings>(config.GetSection("Jwt"));


            // Market data
            services.Configure<TwelveDataSettings>(config.GetSection("TwelveData"));
            services.AddHttpClient<CoinGeckoClient>(c => c.BaseAddress = new Uri("https://api.coingecko.com"));
            services.AddHttpClient<TwelveDataClient>();
            services.AddScoped<IMarketDataService, MarketDataService>();


            services.AddSingleton<IConnectionMultiplexer>(_ =>
                 ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));

            services.AddScoped<ICacheService, RedisCacheService>();

            // AI analysis (Grok via xAI / OpenAI-compatible API)
            services.Configure<GrokSettings>(config.GetSection("Grok"));
            services.AddScoped<IAiAnalysisService, GrokAiAnalysisService>();


            services.AddHttpClient<CoinGeckoClient>(c =>
            {
                c.BaseAddress = new Uri("https://api.coingecko.com");
                c.DefaultRequestHeaders.Add("User-Agent", "TradingAI/1.0");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                             | System.Security.Authentication.SslProtocols.Tls13
            });


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
