using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scrapper.Manager;

namespace Scrapper.Background
{
    public class DailyScraperWorker : BackgroundService
    {
        private readonly ILogger<DailyScraperWorker> _logger;
        private readonly IScraperManager _novelManager;

        public DailyScraperWorker(ILogger<DailyScraperWorker> logger, IScraperManager novelManager)
        {
            _logger = logger;
            _novelManager = novelManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Scraper Worker started.");

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    try
            //    {
            //        _logger.LogInformation("Starting daily scrape at {Time}", DateTime.Now);
            //        await _novelManager.ScrapeLatestNovelsAsync();
            //        _logger.LogInformation("Scrape completed at {Time}", DateTime.Now);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error occurred during scraping.");
            //    }
            //    //await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            //    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            //}
        }
    }
}
