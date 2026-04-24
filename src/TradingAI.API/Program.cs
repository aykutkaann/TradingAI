using FluentValidation;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using TradingAI.API.Middleware;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Infrastructure;
using TradingAI.Infrastructure.AI;
using TradingAI.Infrastructure.OutcomeTracking;
using TradingAI.Infrastructure.Seed;
using TradingAI.Infrastructure.Storage;

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
    typeof(TradingAI.Application.Common.Behaviors.ValidationBehavior<,>));


// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(IApplicationDbContext).Assembly);

//Background service
builder.Services.Configure<OutcomeTrackingSettings>(builder.Configuration.GetSection("OutcomeTracking"));

builder.Services.AddSingleton<IOutcomeEvaluator, OutcomeEvaluator>();
builder.Services.AddHostedService<OutcomeTrackingWorker>();

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

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

var app = builder.Build();

//Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await AssetSeeder.SeedAssetAsync(db);
    await PlanSeeder.SeedPlanAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
