using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Infrastructure.Email
{
    public class EmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
        public string FromName   { get; set; }
        public bool UseSsl { get; set; }
    }
}
