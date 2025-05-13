using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Scrapper.Data.Configs;
using Scrapper.Data.Repositories;
using Scrapper.Helper;
using Scrapper.Interfaces;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Scrapper.Services.ScrapperService
{
    public class SeleniumScrapperService
    {
        private readonly NovelSettings _settings;
        private readonly INovelService _novelService;
        private readonly IChapterService _chapterService;
        private readonly NovelApiClient _apiClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SeleniumScrapperService> _logger;

        private const string ChaptersTitle = "#tab-chapters-title";

        public SeleniumScrapperService(
            IOptions<NovelSettings> options,
            INovelService novelService,
            IChapterService chapterSerivce,
            NovelApiClient apiClient,
            IUnitOfWork unitOfWork,
            ILogger<SeleniumScrapperService> logger)
        {
            _settings = options.Value;
            _novelService = novelService;
            _chapterService = chapterSerivce;
            _apiClient = apiClient;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> GetLastPage(string url)
        {
            try
            {
                using (var driver = new ChromeDriver())
                {
                    driver.Navigate().GoToUrl(url);
                    await Task.Delay(1000); 

                    var lastPageLink = driver.FindElement(By.XPath("//li[contains(@class, 'last')]/a"));
                    if (lastPageLink != null)
                    {
                        var href = lastPageLink.GetAttribute("href");
                        var uri = new Uri(href);
                        var query = HttpUtility.ParseQueryString(uri.Query);
                        if (int.TryParse(query["page"], out int pageNumber))
                        {
                            return pageNumber;
                        }
                    }
                    return 0; 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while scraping the last page from {Url}", url);
                return 0;
            }
        }
    }

}
