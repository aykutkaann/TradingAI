using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Common.Exceptions
{
    public class RateLimitExceededException :Exception
    {
        public int Limit { get; }
        public int Used { get; }
        public string LimitName { get; }

        public RateLimitExceededException(string limitName, int used, int limit) : base($"Daily {limitName} limit reached: {used}/{limit}.")
        {
            LimitName = limitName;
            Limit = limit;
            Used = used;
        }
    }
}
