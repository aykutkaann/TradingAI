using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Domain.Enums
{
    public enum UserRole
    {
        Free = 0,
        Pro = 1,
        Analyst = 2,
        Admin = 99

    }

    public static class SubscriptionLimits
    {
        public static int MonthlyAnalysisCap(UserRole role) => role switch
        {
            UserRole.Free => 10,
            UserRole.Pro => 50,
            UserRole.Analyst => int.MaxValue,
            UserRole.Admin => int.MaxValue,
            _ => 0


        };
    }
}
