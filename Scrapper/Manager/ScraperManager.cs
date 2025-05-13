using Microsoft.Extensions.Options;
using Scrapper.Data.Configs;
using Scrapper.Interfaces;
using Scrapper.Services.ScrapperService;

namespace Scrapper.Manager
{
    public class ScraperManager : IScraperManager
    {
        private readonly ISavingService _savingService;
        private readonly NovelSettings _settings;
        private readonly HtmlAgilityScrapperService _htmlAgilityService;
        private readonly PlaywrightNovelScraper _playwright;
        private readonly SeleniumScrapperService _seliniumScrapper;

        public ScraperManager( ISavingService savingService, IOptions<NovelSettings> settings, HtmlAgilityScrapperService htmlAgilityService,PlaywrightNovelScraper playwright, SeleniumScrapperService selinumScrapper)
        {
            _savingService = savingService;
            _settings = settings.Value;
            _htmlAgilityService = htmlAgilityService;
            _playwright = playwright;
            _seliniumScrapper = selinumScrapper;
        }
        
        //todo: refactor saving logic separately 
        public async Task StartScrapingAsync()
        {
            await _htmlAgilityService.ScrapeAllTitles();
            //await _seliniumScrapper.TestScrape("https://novelbin.me/novel-book/blacksmith-of-the-apocalypse");
        }
    }
}
