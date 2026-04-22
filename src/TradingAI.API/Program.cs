using FluentValidation;
using Serilog;
using TradingAI.API.Middleware;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Infrastructure;
using TradingAI.Infrastructure.Seed;

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

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

//Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await AssetSeeder.SeedAssetAsync(db);
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

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
