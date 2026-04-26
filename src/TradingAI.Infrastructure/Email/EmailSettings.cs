using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Infrastructure.Email
{
    public class EmailSettings
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
        public string FromName { get; set; } = null!;
        public bool UseSsl { get; set; }
    }
}
