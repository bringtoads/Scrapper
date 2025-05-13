using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Scrapper.Data.Configs;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;
using Scrapper.Helper;
using Scrapper.Interfaces;
using System.Text.RegularExpressions;

namespace Scrapper.Services.ScrapperService
{
    public class PlaywrightNovelScraper : IScrapperService
    {
        private readonly NovelSettings _settings;
        private readonly IUnitOfWork _unitOfWork;

        public PlaywrightNovelScraper(IOptions<NovelSettings> options, IUnitOfWork unitOfWork)
        {
            _settings = options.Value;
            _unitOfWork = unitOfWork;
        }
        #region Novels
        public async Task ScrapeAllPages()
        {
            var urls = new[]
            {
                _settings.Latest,
                _settings.Hot,
                _settings.Completed,
                _settings.Popular
            };

            // Create a new Random instance
            Random rand = new Random();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false // Set to true if you don't need to see the browser
            });

            // Create a new browser context and set the user-agent for that context
            var contextOptions = new BrowserNewContextOptions
            {
                UserAgent = GetRandomUserAgent(rand)
            };
            var context = await browser.NewContextAsync(contextOptions);

            var page = await context.NewPageAsync();

            foreach (var url in urls)
            {
                await ScrapeCategoryAsync(page, url, rand);
            }

            Console.WriteLine("Scraping completed!");
        }

        private string GetRandomUserAgent(Random rand)
        {
            string[] userAgents = {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36"
    };
            return userAgents[rand.Next(userAgents.Length)];
        }

        private async Task ScrapeCategoryAsync(IPage page, string url, Random rand)
        {
            Console.WriteLine($"Starting category: {url}");

            // Go to the first page of the category
            await page.GotoAsync(url);

            // Determine the last page number
            int lastPageNumber = await GetLastPageNumberAsync(page, url);
            Console.WriteLine($"Found {lastPageNumber} pages in category.");

            // Loop through all pages
            for (int pageIndex = 1; pageIndex <= lastPageNumber; pageIndex++)
            {
                string pagedUrl = pageIndex == 1 ? url : $"{url}?page={pageIndex}";
                Console.WriteLine($"Scraping page {pageIndex}: {pagedUrl}");

                await ScrapePageAsync(page, pagedUrl);

                // Randomized delay to avoid rate limiting
                int delayTime = rand.Next(1000, 5000); // Random delay between 1 and 5 seconds
                await Task.Delay(delayTime);
            }

            Console.WriteLine($"Finished scraping category: {url}");
        }

        private async Task<int> GetLastPageNumberAsync(IPage page, string url)
        {
            var lastLink = await page.QuerySelectorAsync("li.last a");

            if (lastLink != null)
            {
                var href = await lastLink.GetAttributeAsync("href");
                if (!string.IsNullOrEmpty(href))
                {
                    var match = Regex.Match(href, @"page=(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int pageNumber))
                    {
                        return pageNumber;
                    }
                }
            }

            return 1; // Default to 1 if no last page is found
        }

        private async Task ScrapePageAsync(IPage page, string pagedUrl)
        {
            await page.GotoAsync(pagedUrl);
            await page.WaitForSelectorAsync("div.list.list-novel.col-xs-12");

            var rows = await page.Locator("div.list.list-novel.col-xs-12 div.row").AllAsync();
            Console.WriteLine($"Found {rows.Count} novels on page {pagedUrl}.");

            foreach (var row in rows)
            {
                var titleElement = row.Locator("h3.novel-title a");
                var title = await titleElement.InnerTextAsync();
                var novelurl = await titleElement.GetAttributeAsync("href");

                var authorElement = row.Locator("span.author");
                var author = await authorElement.InnerTextAsync();

                Console.WriteLine($"Saving novel: {title} by {author} - {novelurl}");

                await SaveNovelAndAuthorAsync(title, author, novelurl);
            }
        }


        public Task ScrapePage(string url, int page)
        {
            throw new NotImplementedException();
        }
        public Task ScrapeLatestNovels()
        {
            throw new NotImplementedException();
        }

        public Task ScrapeHotestNovels()
        {
            throw new NotImplementedException();
        }

        public Task ScrapeCompletedNovels()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Chapters
        public async Task ScrapeChapterTitleUrl(string chapterTitlesUrl, int novelId)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
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


        #endregion

        private async Task SaveNovelAndAuthorAsync(string novelTitle, string authorName, string novelSourceUrl)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var author = await _unitOfWork.AuthorRepository.GetByNameAsync(authorName);
                if (author == null)
                {
                    author = new Author { Name = authorName };
                    await _unitOfWork.AuthorRepository.AddAsync(author);
                    await _unitOfWork.CompleteAsync();
                }

                var existingNovel = await _unitOfWork.NovelRepository.GetBySourceUrlAsync(novelSourceUrl);
                if (existingNovel == null)
                {
                    var novel = new Novel
                    {
                        Title = novelTitle,
                        SourceUrl = novelSourceUrl,
                        AuthorId = author.AuthorId
                    };

                    await _unitOfWork.NovelRepository.AddAsync(novel);
                    await _unitOfWork.CompleteAsync();

                }
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
            }
        }
    }

}
