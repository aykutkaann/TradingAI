using TradingAI.Application.Assets.DTOs;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Watchlist.DTOs;

public record WatchlistItemDto(
    Guid Id,
    Guid AssetId,
    string Symbol,
    string Name,
    string Pair,
    AssetType Type,
    int SortOrder,
    PriceDto? Price
);
