using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Domain.Enums;

namespace TradingAI.Infrastructure.OutcomeTracking
{
    public class OutcomeTrackingWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<OutcomeTrackingSettings> _settings;
        private readonly ILogger<OutcomeTrackingWorker> _logger;

        public OutcomeTrackingWorker(IServiceScopeFactory scopeFactory, IOptions<OutcomeTrackingSettings> settings,
            ILogger<OutcomeTrackingWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutcomeTrackingWorker starting.");


            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }catch(OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }catch(Exception err)
                {
                    _logger.LogError(err, "Outcome tick failed");
                }

                try
                {
                    await Task.Delay(
                        TimeSpan.FromMinutes(_settings.Value.PollIntervalMinutes),
                        stoppingToken);
                }
                catch (OperationCanceledException) { break; }


            }
            _logger.LogInformation("OutcomeTrackingWorker stopping.");


        }

        private async Task ProcessBatchAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var market = scope.ServiceProvider.GetRequiredService<IMarketDataService>();
            var evaluator = scope.ServiceProvider.GetRequiredService<IOutcomeEvaluator>();

            var batch = await db.Analyses.Where(a => a.Outcome == Domain.Enums.AnalysisOutcome.Pending)
                .OrderBy(a => a.OutcomeCheckedAt ?? DateTime.MinValue)
                .Take(_settings.Value.BatchSize)
                .ToListAsync(ct);

            if (batch.Count == 0)
            {
                _logger.LogDebug("No pending analyses this tick.");
                return;
            }

            int resolvedCount = 0;


            foreach (var analysis in batch)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var fromUtc = analysis.OutcomeCheckedAt ?? analysis.CreatedAt;
                    var toUtc = DateTime.UtcNow;

                    var candles = await market.GetCandlesAsync(
                        analysis.Pair, analysis.TimeFrame, fromUtc, toUtc, ct);

                    var result = evaluator.Evaluate(analysis, candles, DateTime.UtcNow);

                    analysis.TakeProfit1Hit = result.TakeProfit1Hit; 
                    analysis.TakeProfit2Hit = result.TakeProfit2Hit;
                    analysis.StopLossHit = result.StopLossHit;
                    analysis.Outcome = result.Outcome;
                    analysis.OutcomeCheckedAt = DateTime.UtcNow;

                    if (result.Outcome != AnalysisOutcome.Pending)
                    {
                        analysis.ResolvedPrice = result.ResolvedPrice;
                        analysis.ResolvedAt = result.ResolvedAt;
                        resolvedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to evaluate analysis {AnalysisId}", analysis.Id);
                    analysis.OutcomeCheckedAt = DateTime.UtcNow; // avoid instant retry
                }
            }

            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Outcome tick: processed {Count}, resolved {Resolved}",
                batch.Count, resolvedCount);


        }
    }
}
