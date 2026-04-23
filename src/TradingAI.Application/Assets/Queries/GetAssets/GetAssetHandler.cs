using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Assets.DTOs;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Assets.Queries.GetAssets
{
    public class GetAssetHandler : IRequestHandler<GetAssetsQuery,IReadOnlyList<AssetDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetAssetHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<AssetDto>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
        {

            return await _db.Assets.AsNoTracking().Where(a => a.IsActive && (request.Type == null || a.Type == request.Type))
                                    .Select(a => new AssetDto(
                                        a.Id,
                                        a.Symbol,
                                        a.Name,
                                        a.Pair,
                                        a.Type)).ToListAsync(cancellationToken);
        }
    }
}
