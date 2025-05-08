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

        public ScraperManager( ISavingService savingService, IOptions<NovelSettings> settings, HtmlAgilityScrapperService htmlAgilityService)
        {
            _savingService = savingService;
            _settings = settings.Value;
            _htmlAgilityService = htmlAgilityService;
        }
        
        //todo: refactor saving logic separately 
        public async Task StartScrapingAsync()
        {
            await _htmlAgilityService.ScrapeAllPages();
        }
    }
}
