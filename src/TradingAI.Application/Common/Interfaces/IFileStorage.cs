using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Common.Interfaces
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(Stream stream, string fileName, string userId, CancellationToken ct);
        Task DeleteAsync(string url, CancellationToken ct);
    }
}
