using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Scrapper.Data.Configs;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;
using Scrapper.Helper;
using Scrapper.Interfaces;

namespace Scrapper.Services.ScrapperService
{
    internal class PlaywrightNovelScraper : IScrapperService
    {
        private readonly NovelSettings _settings;
        private readonly INovelService _novelService;
        private readonly IChapterService _chapterService;
        private readonly IUnitOfWork _unitOfWork;
        public PlaywrightNovelScraper(IOptions<NovelSettings> options,INovelService novelService, IChapterService chapterSerivce, IUnitOfWork unitOfWOrk)
        {
            _settings = options.Value;
            _novelService = novelService;
            _chapterService = chapterSerivce;
            _unitOfWork = unitOfWOrk;
        }

        public async Task<string> GetPageTitle(string url)
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();

            await page.GotoAsync(url);
            await page.WaitForSelectorAsync("#some-element");
            var title = await page.TitleAsync();

            await browser.CloseAsync();
            return title;
        }
        public async Task ScrapeChapterTitleUrl(string chapterTitlesUrl, int novelId)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync(chapterTitlesUrl);
            await page.WaitForSelectorAsync("#tab-chapters");

            var chapterTab = page.Locator("#tab-chapters .tab-title");
            if (await chapterTab.IsVisibleAsync())
            {
                await chapterTab.ClickAsync();
                await page.WaitForSelectorAsync("#list-chapter");
            }

            var liNodes = await page.Locator("#list-chapter ul.list-chapter li").AllAsync();

            foreach (var li in liNodes)
            {
                var aTag = li.Locator("a");
                var title = (await aTag.GetAttributeAsync("title") ?? "").Trim();
                var href = (await aTag.GetAttributeAsync("href") ?? "").Trim();

                var contentFilepath = await ScrapeChapterContent(href);

                var chapter = new Chapter
                {
                    Title = title,
                    NovelId = novelId,
                    FilePath = contentFilepath,
                    SourceUrl = href
                };

                await _unitOfWork.ChapterRepository.AddAsync(chapter);
                Console.WriteLine($"{title}: {contentFilepath}");
            }

            await browser.CloseAsync();
        }

        public async Task<string> ScrapeChapterContent(string novelChapterUrl)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync(novelChapterUrl);
            await page.WaitForSelectorAsync("#chr-content");

            var paragraphs = await page.Locator("#chr-content p").AllInnerTextsAsync();
            var content = string.Join(Environment.NewLine + Environment.NewLine, paragraphs.Select(p => p.Trim()));

            var urlParts = novelChapterUrl.Split('/');
            string novelName = urlParts[4];
            string chapterName = urlParts[5];

            var filepath = NovelSaver.SaveChapter(novelName, chapterName, content);
            await browser.CloseAsync();
            return filepath;
        }
    }
}
