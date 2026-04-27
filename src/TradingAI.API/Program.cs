using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using TradingAI.API.Middleware;
using TradingAI.API.Services;
using TradingAI.Application.Common.Behaviors;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Infrastructure;
using TradingAI.Infrastructure.AI;
using TradingAI.Infrastructure.OutcomeTracking;
using TradingAI.Infrastructure.Seed;
using TradingAI.Infrastructure.Storage;

// Npgsql 6+ rejects DateTime values with Kind != Utc when writing to
// `timestamp with time zone` columns. We have a few code paths (outcome
// evaluator's resolvedAt, candle timestamps from market-data providers)
// that produce DateTimeKind.Unspecified. Rather than audit every site,
// flip the legacy switch — Npgsql will then treat Unspecified as UTC.
// See: https://www.npgsql.org/efcore/release-notes/6.0.html#timestamp-rationalization
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, config) =>
    config.ReadFrom.Configuration(ctx.Configuration)
          .WriteTo.Console());

// Infrastructure (DbContext, JWT, IPasswordHasher, IJwtService, IApplicationDbContext)
builder.Services.AddInfrastructure(builder.Configuration);

// MediatR + validation pipeline
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IApplicationDbContext).Assembly));
builder.Services.AddTransient(
    typeof(MediatR.IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RateLimitBehavior<,>));



// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(IApplicationDbContext).Assembly);

//Background service
builder.Services.Configure<OutcomeTrackingSettings>(builder.Configuration.GetSection("OutcomeTracking"));

builder.Services.AddSingleton<IOutcomeEvaluator, OutcomeEvaluator>();
builder.Services.AddHostedService<OutcomeTrackingWorker>();

//CurrentUser 
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Health checks. Both connection strings are optional at registration —
// missing values just mean the corresponding check isn't registered.
// This lets the app boot far enough to log a meaningful error instead of
// dying on a null-arg exception inside the health check builder.
var pgConn = builder.Configuration.GetConnectionString("DefaultConnection");
var redisConn = builder.Configuration.GetConnectionString("Redis");

if (string.IsNullOrWhiteSpace(pgConn))
{
    Console.WriteLine(
        "[STARTUP] ERROR: ConnectionStrings__DefaultConnection env var is not set. " +
        "App will boot but every DB request will fail. " +
        "Set this variable on your hosting platform (Railway, Fly, etc).");
}

var healthChecks = builder.Services.AddHealthChecks();
if (!string.IsNullOrWhiteSpace(pgConn)) healthChecks.AddNpgSql(pgConn);
if (!string.IsNullOrWhiteSpace(redisConn)) healthChecks.AddRedis(redisConn);

// Forwarded headers — required behind reverse proxies (Railway, Fly, Nginx)
// so HttpContext.Request.Scheme reflects the original https:// from the client,
// not the http:// hop inside the platform's network.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

//CORS


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS — origins come from configuration so dev and prod can each list
// the URLs they need. Set "Cors:AllowedOrigins" to a comma-separated list
// (or as a JSON array in appsettings, or the env var
//   Cors__AllowedOrigins__0, Cors__AllowedOrigins__1, ...).


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://trading-ai-seven.vercel.app/")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



var app = builder.Build();

// Run pending migrations + seed reference data on startup. Idempotent.
// Wrapped in a separate scope so the DI scope lifetime is correct.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await AssetSeeder.SeedAssetAsync(db);
    await PlanSeeder.SeedPlanAsync(db);
}

// Trust forwarded headers BEFORE any middleware that reads the URL scheme.
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// HTTPS redirect only in dev. In production the platform (Railway / Fly / Nginx)
// terminates TLS at the edge and the app receives plain HTTP internally —
// keeping UseHttpsRedirection on would cause a redirect loop or break the app.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
