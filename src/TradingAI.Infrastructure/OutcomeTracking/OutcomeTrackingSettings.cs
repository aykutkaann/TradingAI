using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Infrastructure.OutcomeTracking
{
    public class OutcomeTrackingSettings
    {
        public int PollIntervalMinutes { get; set; } = 5;
        public int BatchSize { get; set; } = 100;
        public int DefaultExpiryDays { get; set; } = 7;

    }
}
