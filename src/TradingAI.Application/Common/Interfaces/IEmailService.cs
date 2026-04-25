using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
    }
}
