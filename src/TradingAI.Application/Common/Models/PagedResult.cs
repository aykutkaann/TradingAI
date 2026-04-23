using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Common.Models
{

    public record PagedResult<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int Page,
        int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrev => Page > 1;
    }
}
