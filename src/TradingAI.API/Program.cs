using Microsoft.EntityFrameworkCore;
using TradingAI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

//DB connection
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();





app.Run();

