using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scrapper.Data.Configs;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;
using Scrapper.Helper;
using Scrapper.Interfaces;
using System.Net;
using System.Text.RegularExpressions;

namespace Scrapper.Services.ScrapperService
{
    public class HtmlAgilityScrapperService
    {
        private readonly NovelSettings _settings;
        private readonly INovelService _novelService;
        private readonly IChapterService _chapterService;
        private readonly NovelApiClient _apiClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly SeleniumScrapperService _seleniumScrapper;
        private readonly ILogger<HtmlAgilityScrapperService> _logger;
        private string ChaptersTitle = "#tab-chapters-title";

        public HtmlAgilityScrapperService(
            IOptions<NovelSettings> options,
            INovelService novelService,
            IChapterService chapterService,
            NovelApiClient apiClient,
            IUnitOfWork unitOfWork,
            SeleniumScrapperService seleniumScrapper,
            ILogger<HtmlAgilityScrapperService> logger)
        {
            _settings = options.Value;
            _novelService = novelService;
            _chapterService = chapterService;
            _apiClient = apiClient;
            _unitOfWork = unitOfWork;
            _seleniumScrapper = seleniumScrapper;
            _logger = logger;
        }
       
        public async Task ScrapeAll()
        {
            var urls = new[]
            {
                _settings.Latest,
                _settings.Hot,
                _settings.Completed,
                _settings.Popular
            };
            foreach (var url in urls)
            {
                await ScarapeList(url);
            }
        }
        public async Task ScarapeList(string url)
        {
            string html = string.Empty;

            if (url == _settings.Latest)
            {
                html = await _apiClient.GetLatest();
            }
            else if (url == _settings.Hot)
            {
                html = await _apiClient.GetHot();
            }
            else if (url == _settings.Completed)
            {
                html = await _apiClient.GetCompleted();
            }
            else if (url == _settings.Popular)
            {
                html = await _apiClient.GetPopular();
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            int maxPage = await _seleniumScrapper.GetLastPage(url);

            for (int i = 0; i <= maxPage; i++)
            {
                await ScrapeRows(i, url);
            }
        }

        private async Task ScrapeRows(int pageNum, string url)
        {
            string html = string.Empty;
            if (url == _settings.Latest)
            {
                html = await _apiClient.Get(_settings.NavLatest + pageNum);
            }
            else if (url == _settings.Hot)
            {
                html = await _apiClient.Get(_settings.NavHot + pageNum);
            }
            else if (url == _settings.Completed)
            {
                html = await _apiClient.Get(_settings.NavCompleted + pageNum);
            }
            else if (url == _settings.Popular)
            {
                html = await _apiClient.Get(_settings.NavPopular + pageNum);
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var rows = doc.DocumentNode.SelectNodes("//div[@class='list list-novel col-xs-12']/div[@class='row']");

            if (rows is null)
            {
                return;
            }

            foreach (var row in rows)
            {
                var titleNode = row.SelectSingleNode(".//div[@class='col-xs-7']//h3[@class='novel-title']/a");
                var authorNode = row.SelectSingleNode(".//div[@class='col-xs-7']//span[@class='author']");

                if (titleNode is null || authorNode is null)
                    continue;

                var title = WebUtility.HtmlDecode(titleNode.InnerText.Trim());
                var authorName = WebUtility.HtmlDecode(authorNode.InnerText.Replace("glyphicon-pencil", "").Trim());
                var sourceUrl = titleNode.GetAttributeValue("href", "");
                var (description, image) = await ScrapeNovelDescriptionAndImage(sourceUrl);

                await SaveNovelAndAuthorAsync(title, authorName, sourceUrl, description, image);
            }
        }

        public async Task<(string Description, byte[] ImageData)> ScrapeNovelDescriptionAndImage(string sourceUrl, int retryCount = 0)
        {
            const int maxRetries = 5;

            try
            {
                // Get the HTML content
                var html = await _apiClient.Get(sourceUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Extract description
                string description = await ExtractDescriptionAsync(doc, sourceUrl, retryCount, maxRetries);

                // Extract and download image
                byte[] imageData = await ExtractAndDownloadImageAsync(doc, sourceUrl);

                return (description, imageData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping novel: {ex.Message}");
                return (string.Empty, Array.Empty<byte>());
            }
        }


        public async Task ScrapeAllTitles()
        {
            var novels = await _novelService.GetAllNovels();
            foreach (var novel in novels)
            {
                await ScrapeChapterTitleUrl(novel.SourceUrl + ChaptersTitle, novel.NovelId);
            }
        }

        public async Task ScrapeChapterTitleUrl(string chapterTitlesUrl, int novelId)
        {
            var listOfChapterTitles = await _seleniumScrapper.ScrapeDynamicChapterTitleUrl(chapterTitlesUrl);
            if (listOfChapterTitles is not null)
            {
                foreach (var title in listOfChapterTitles)
                {
                    var (contentFilepath, chapaterTitle) = await ScrapeChapterContent(title.Url);
                    var (chapterNumber, cleanTitle) = ExtractChapterInfo(chapaterTitle);
                    var nullChapterNumber = string.IsNullOrEmpty(chapterNumber) ? "0" : chapterNumber;
                    var chapter = new Chapter
                    {
                        Title = chapaterTitle,
                        ChapterNumber = decimal.Parse(nullChapterNumber),
                        NovelId = novelId,
                        FilePath = contentFilepath,
                        SourceUrl = title.Url
                    };
                    await _unitOfWork.ChapterRepository.AddAsync(chapter);
                }
            }
        }

        public async Task<(string filepath, string chapterTitle)> ScrapeChapterContent(string novelChapterUrl, int retryCount = 0)
        {
            const int maxRetries = 5;

            var html = await _apiClient.Get(novelChapterUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var unlockButtonsDiv = doc.DocumentNode.SelectSingleNode("//div[@class='unlock-buttons text-center']");
            unlockButtonsDiv?.Remove();

            var chrContentDiv = doc.DocumentNode.SelectSingleNode("//*[@id='chr-content']");

            if (chrContentDiv is null && retryCount < maxRetries)
            {
                await Task.Delay(2000); // Wait for 2 seconds before retrying
                return await ScrapeChapterContent(novelChapterUrl, retryCount + 1);
            }

            if (chrContentDiv is not null)
            {
                var chapterTitleSpanNode = doc.DocumentNode.SelectSingleNode("//a[@class='chr-title']/span[@class='chr-text']");
                var chapterTitleH4Tag = chrContentDiv.SelectSingleNode(".//h4");
                var chapterTitleH3Tag = chrContentDiv.SelectSingleNode(".//h3");
                string chapterTitle = chapterTitleSpanNode != null ? chapterTitleSpanNode.InnerHtml.Trim() : chapterTitleH4Tag != null ? chapterTitleH4Tag.InnerText.Trim() : (chapterTitleH3Tag != null ? chapterTitleH3Tag.InnerText.Trim() : string.Empty);

                // If no <h4>, check the first <p> tag and match the format "Chapter <number>: <some title>"
                if (string.IsNullOrEmpty(chapterTitle))
                {
                    var firstPTag = chrContentDiv.Descendants("p").FirstOrDefault();
                    if (firstPTag != null)
                    {
                        var firstPText = firstPTag.InnerText.Trim();
                        var match = Regex.Match(firstPText, @"^Chapter\s+(\d+):\s+(.*)$");

                        if (match.Success)
                        {
                            chapterTitle = match.Groups[2].Value; // Extract the title part after "Chapter <number>:"
                        }
                    }
                }

                // Extract the content from all <p> tags
                var pTags = chrContentDiv.Descendants("p");
                string content = "";

                foreach (var p in pTags)
                {
                    content += p.InnerText.Trim() + Environment.NewLine + Environment.NewLine;
                }

                // Parse novelName and chapterName from the URL
                var urlParts = novelChapterUrl.Split('/');
                string novelName = urlParts[4];      // skill-hunter-kill-monsters-acquire-skills-ascend-to-the-highest-rank
                string chapterName = urlParts[5];    // 1-dark-place

                // Use your NovelSaver here
                var filepath = NovelSaver.SaveChapter(novelName, chapterName, content);
                return (filepath, chapterTitle);
            }
            else
            {
                // If content is still not found after retries, return empty
                return (string.Empty, string.Empty);
            }
        }

        private async Task SaveNovelAndAuthorAsync(string novelTitle, string authorName, string novelSourceUrl, string description, byte[] image)
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
                        Description = description,
                        CoverImage = image,
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

        private (string chapterNumber, string cleanTitle) ExtractChapterInfo(string fullTitle)
        {
            if (string.IsNullOrWhiteSpace(fullTitle))
                return (string.Empty, string.Empty);

            // Example: "Chapter 1 - 1.The World Ended?" or "Chapter 12: A New Beginning"
            var regex = new Regex(@"Chapter\s*(\d+)[^\w]*(?:\d*\.)?(.*)", RegexOptions.IgnoreCase);
            var match = regex.Match(fullTitle);

            if (match.Success)
            {
                var chapterNumber = match.Groups[1].Value.Trim();
                var cleanTitle = match.Groups[2].Value.Trim();
                return (chapterNumber, cleanTitle);
            }

            // fallback if format is unexpected
            return (string.Empty, fullTitle.Trim());
        }

        private async Task<string> ExtractDescriptionAsync(HtmlDocument doc, string sourceUrl, int retryCount, int maxRetries)
        {
            var descDiv = doc.DocumentNode.SelectSingleNode("//div[@class='desc-text']");

            // Retry logic for description
            if (descDiv is null && retryCount < maxRetries)
            {
                await Task.Delay(2000);
                var (description, _) = await ScrapeNovelDescriptionAndImage(sourceUrl, retryCount + 1);
                return description;
            }

            if (descDiv is not null)
            {
                var description = descDiv.InnerHtml;
                description = Regex.Replace(description, @"\s+", " ").Trim();
                return description;
            }

            return string.Empty;
        }

        private async Task<byte[]> ExtractAndDownloadImageAsync(HtmlDocument doc, string sourceUrl)
        {
            // Try to find the image node using different selectors
            var imageNode = FindImageNode(doc);
            if (imageNode == null)
            {
                Console.WriteLine("Image node not found using any selector");
                return Array.Empty<byte>();
            }

            // Extract image URL from various attributes
            string imageUrl = ExtractImageUrl(imageNode);
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return Array.Empty<byte>();
            }

            // Ensure URL is absolute
            imageUrl = EnsureAbsoluteUrl(imageUrl, sourceUrl);

            // Download the image
            return await DownloadImageAsync(imageUrl);
        }

        private HtmlNode FindImageNode(HtmlDocument doc)
        {
            // Try selectors from most specific to most general
            var selectors = new[]
            {
                "//div[@id='novel']//div[contains(@class, 'col-novel-main')]//div[contains(@class, 'col-info-desc')]//div[contains(@class, 'info-holder')]//div[@class='books']//div[@class='book']//img[@class='lazy']",
                "//div[@class='books']//div[@class='book']//img[@class='lazy']", "//div[@class='book']/img"
            };

            foreach (var selector in selectors)
            {
                var node = doc.DocumentNode.SelectSingleNode(selector);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        private string ExtractImageUrl(HtmlNode imageNode)
        {
            var attributesToCheck = new[] { "src", "data-src", "data-original" };

            foreach (var attribute in attributesToCheck)
            {
                var url = imageNode.GetAttributeValue(attribute, "");
                if (!string.IsNullOrWhiteSpace(url))
                {
                    return url;
                }
            }

            return string.Empty;
        }

        private string EnsureAbsoluteUrl(string imageUrl, string baseUrl)
        {
            if (!imageUrl.StartsWith("http"))
            {
                Uri baseUri = new Uri(baseUrl);
                return new Uri(baseUri, imageUrl).ToString();
            }

            return imageUrl;
        }

        private async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            try
            {
                return await ImageUtility.DownloadImageAsync(imageUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download image: {ex.Message}");
                return Array.Empty<byte>();
            }
        }
    }
}
