using FluentValidation;
using MediatR;
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

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
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
app.UseCors("AllowDevFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
