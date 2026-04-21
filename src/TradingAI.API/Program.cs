using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using TradingAI.API.Middleware;
using TradingAI.Infrastructure;

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

