using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Scrapper.Data.Configs;
using Scrapper.Data.Repositories;
using Scrapper.Helper;
using Scrapper.Interfaces;
using System.Web;
using Microsoft.Extensions.Logging;
using Scrapper.Data.Entity;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Support.UI;
using Scrapper.Contracts.DTOs;

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
                    return 0; // Default if no last page found
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while scraping the last page from {Url}", url);
                return 0;
            }
        }


        public async Task<List<ChpaterTitleDto>?> ScrapeDynamicChapterTitleUrl(string chapterTitlesUrl)
        {
            var list = new List<ChpaterTitleDto>();
            var options = new ChromeOptions();
            options.AddArguments("--headless"); // Run headless, remove if you need a UI.
            using var driver = new ChromeDriver(options);

            driver.Navigate().GoToUrl(chapterTitlesUrl);
            await Task.Delay(2000);
            // Wait for the panel-body to load (adjust the timeout as needed)
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var panelBody = wait.Until(d => d.FindElement(By.XPath("//div[contains(@class, 'panel-body')]")));

            if (panelBody == null) return null;

            // Wait for the row divs to load
            var rowDivs = wait.Until(d => d.FindElements(By.XPath(".//div[contains(@class, 'row')]")));
            if (rowDivs == null || rowDivs.Count == 0) return null;

            foreach (var row in rowDivs)
            {
                var colDivs = row.FindElements(By.XPath(".//div[contains(@class, 'col-xs-12') and contains(@class, 'col-sm-4') and contains(@class, 'col-md-4')]"));
                if (colDivs == null || colDivs.Count == 0) continue;

                foreach (var col in colDivs)
                {
                    var liNodes = col.FindElements(By.XPath(".//ul[contains(@class, 'list-chapter')]/li"));
                    if (liNodes == null || liNodes.Count == 0) continue;

                    foreach (var li in liNodes)
                    {
                        var aTag = li.FindElement(By.XPath(".//a"));
                        if (aTag == null) continue;

                        var chapterUrl = aTag.GetAttribute("href").Trim();
                        var chapter = new ChpaterTitleDto {
                            Url = chapterUrl,
                            Title = string.Empty
                        };
                        list.Add(chapter);
                    }
                }
            }

            driver.Quit(); // Ensure we properly close the browser session
            return list;
        } 
    }
}