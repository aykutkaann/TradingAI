using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Infrastructure
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) :DbContext(options)
    {

    }
}
