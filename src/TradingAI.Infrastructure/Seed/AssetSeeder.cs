using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Domain.Entities;
using TradingAI.Domain.Enums;

namespace TradingAI.Infrastructure.Seed
{
    public static class AssetSeeder
    {
        public static async Task SeedAssetAsync(IApplicationDbContext db)
        {
            if (await db.Assets.AnyAsync()) return;

            var assets = new List<Asset>();

            // 1. CoinGecko IDs
            assets.AddRange(new[]
            {
                new Asset { Name = "Bitcoin", Symbol = "BTC/USDT", DataSourceId = "bitcoin", Type = AssetType.Crypto },
                new Asset { Name = "Ethereum", Symbol = "ETH/USDT", DataSourceId = "ethereum", Type = AssetType.Crypto },
                new Asset { Name = "Solana", Symbol = "SOL/USDT", DataSourceId = "solana", Type = AssetType.Crypto },
                new Asset { Name = "Ripple", Symbol = "XRP/USDT", DataSourceId = "ripple", Type = AssetType.Crypto },
                new Asset { Name = "BNB", Symbol = "BNB/USDT", DataSourceId = "binancecoin", Type = AssetType.Crypto },
                new Asset { Name = "Cardano", Symbol = "ADA/USDT", DataSourceId = "cardano", Type = AssetType.Crypto },
                new Asset { Name = "Dogecoin", Symbol = "DOGE/USDT", DataSourceId = "dogecoin", Type = AssetType.Crypto },
                new Asset { Name = "Polkadot", Symbol = "DOT/USDT", DataSourceId = "polkadot", Type = AssetType.Crypto },
                new Asset { Name = "Chainlink", Symbol = "LINK/USDT", DataSourceId = "chainlink", Type = AssetType.Crypto },
                new Asset { Name = "Avalanche", Symbol = "AVAX/USDT", DataSourceId = "avalanche-2", Type = AssetType.Crypto }
            });

            // 2. Forex (Twelve Data symbols)
            assets.AddRange(new[]
            {
                new Asset { Name = "Euro / US Dollar", Symbol = "EUR/USD", Type = AssetType.Forex },
                new Asset { Name = "British Pound / US Dollar", Symbol = "GBP/USD", Type = AssetType.Forex },
                new Asset { Name = "US Dollar / Turkish Lira", Symbol = "USD/TRY", Type = AssetType.Forex },
                new Asset { Name = "US Dollar / Japanese Yen", Symbol = "USD/JPY", Type = AssetType.Forex }
            });

            // 3. Commodity
            assets.AddRange(new[]
            {
                new Asset { Name = "Gold", Symbol = "XAU/USD", Type = AssetType.Commodity },
                new Asset { Name = "Silver", Symbol = "XAG/USD", Type = AssetType.Commodity },
                new Asset { Name = "WTI Crude Oil", Symbol = "WTI", Type = AssetType.Commodity }
            });


            await db.Assets.AddRangeAsync(assets);
            await db.SaveChangesAsync(default);
        }
    }
}
