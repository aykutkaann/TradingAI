using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Domain.Entities;
using TradingAI.Domain.Enums;

namespace TradingAI.Infrastructure.Seed;

public static class AssetSeeder
{
    public static async Task SeedAssetAsync(IApplicationDbContext db)
    {
        if (await db.Assets.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var assets = new List<Asset>();

        // 1. Crypto (CoinGecko)
        assets.AddRange(new[]
        {
            new Asset { Id = Guid.NewGuid(), Symbol = "BTC",   Name = "Bitcoin",    Pair = "BTC/USDT",  DataSourceId = "bitcoin",     Type = AssetType.Crypto, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "ETH",   Name = "Ethereum",   Pair = "ETH/USDT",  DataSourceId = "ethereum",    Type = AssetType.Crypto, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "SOL",   Name = "Solana",     Pair = "SOL/USDT",  DataSourceId = "solana",      Type = AssetType.Crypto, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "XRP",   Name = "Ripple",     Pair = "XRP/USDT",  DataSourceId = "ripple",      Type = AssetType.Crypto, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "BNB",   Name = "BNB",        Pair = "BNB/USDT",  DataSourceId = "binancecoin", Type = AssetType.Crypto, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "ADA",   Name = "Cardano",    Pair = "ADA/USDT",  DataSourceId = "cardano",     Type = AssetType.Crypto, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "DOGE",  Name = "Dogecoin",   Pair = "DOGE/USDT", DataSourceId = "dogecoin",    Type = AssetType.Crypto, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "DOT",   Name = "Polkadot",   Pair = "DOT/USDT",  DataSourceId = "polkadot",    Type = AssetType.Crypto, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "LINK",  Name = "Chainlink",  Pair = "LINK/USDT", DataSourceId = "chainlink",   Type = AssetType.Crypto, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "AVAX",  Name = "Avalanche",  Pair = "AVAX/USDT", DataSourceId = "avalanche-2", Type = AssetType.Crypto, CreatedAt = now },
        });

        // 2. Forex (Twelve Data — Pair == DataSourceId)
        assets.AddRange(new[]
        {
            new Asset { Id = Guid.NewGuid(), Symbol = "EURUSD", Name = "Euro / US Dollar",        Pair = "EUR/USD", DataSourceId = "EUR/USD", Type = AssetType.Forex, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "GBPUSD", Name = "British Pound / USD",     Pair = "GBP/USD", DataSourceId = "GBP/USD", Type = AssetType.Forex, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "USDTRY", Name = "US Dollar / Turkish Lira",Pair = "USD/TRY", DataSourceId = "USD/TRY", Type = AssetType.Forex, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "USDJPY", Name = "US Dollar / Japanese Yen",Pair = "USD/JPY", DataSourceId = "USD/JPY", Type = AssetType.Forex, CreatedAt = now },
        });

        // 3. Commodities (Twelve Data)
        assets.AddRange(new[]
        {
            new Asset { Id = Guid.NewGuid(), Symbol = "XAU", Name = "Gold",          Pair = "XAU/USD", DataSourceId = "XAU/USD", Type = AssetType.Commodity, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "XAG", Name = "Silver",        Pair = "XAG/USD", DataSourceId = "XAG/USD", Type = AssetType.Commodity, CreatedAt = now },
            new Asset { Id = Guid.NewGuid(), Symbol = "WTI", Name = "WTI Crude Oil", Pair = "WTI/USD", DataSourceId = "WTI/USD", Type = AssetType.Commodity, CreatedAt = now },
        });




        await db.Assets.AddRangeAsync(assets);
        await db.SaveChangesAsync(default);
    }
}
