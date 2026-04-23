namespace TradingAI.API.Watchlist.DTOs;

public record AddToWatchlistDto(Guid AssetId);

public record ReorderWatchlistDto(IReadOnlyList<Guid> AssetIds);
