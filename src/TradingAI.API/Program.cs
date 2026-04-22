using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using TradingAI.API.Middleware;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Infrastructure;
using TradingAI.Infrastructure.Auth;
using TradingAI.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

//DB connection
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

builder.Host.UseSerilog((ctx, config) =>
    config.ReadFrom.Configuration(ctx.Configuration)
          .WriteTo.Console());

//MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(IApplicationDbContext).Assembly);
});

//FluentValidation

builder.Services.AddValidatorsFromAssembly(typeof(IApplicationDbContext).Assembly);

//Interfaces
builder.Services.AddScoped<IApplicationDbContext, AppDbContext>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

//JWT

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IJwtService, JwtService>();


builder.Services.AddControllers();
// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();



var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

//Endpoints
app.MapHealthChecks("/health");

app.MapGet("/health-1", () =>
{
    Console.WriteLine("API is running");

    return Results.Ok("Api is running");
});

app.Run();

