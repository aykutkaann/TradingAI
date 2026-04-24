using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Users.DTOs;

public record UserStatsDto(int TotalAnalyses, int PublishedAnalyses, int Wins, int Losses, int Expired, int Pending,
    double WinRate, int Tp2HitCount, double Tp2HitRate);



