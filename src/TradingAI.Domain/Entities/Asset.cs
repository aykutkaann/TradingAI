using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Domain.Entities
{
    public class Asset
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; } = null!;        // "BTC", "XAU", "EUR"
        public string Name { get; set; } = null!;         // "Bitcoin", "Gold", "Euro"
        public AssetType Type { get; set; }        // Crypto, Forex, Commodity, Stock
        public string Pair { get; set; } = null!;         // "BTC/USDT", "XAU/USD", "EUR/TRY"
        public string DataSourceId { get; set; } = null!;   // CoinGecko ID or Twelve Data symbol
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
