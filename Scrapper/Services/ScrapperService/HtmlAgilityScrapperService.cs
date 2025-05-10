using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
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
        private string ChaptersTitle = "#tab-chapters-title";

        public HtmlAgilityScrapperService(IOptions<NovelSettings> options, INovelService novelService, IChapterService chapterSerivce, NovelApiClient apiClient, IUnitOfWork unitOfWork)
        {
            _settings = options.Value;
            _novelService = novelService;
            _chapterService = chapterSerivce;
            _apiClient = apiClient;
            _unitOfWork = unitOfWork;
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

            var lastPageNode = doc.DocumentNode.SelectSingleNode("//li[normalize-space(@class)='last']/a");
            int maxPage = 0;

            if (lastPageNode is not null)
            {
                var lastPageUrl = WebUtility.HtmlDecode(lastPageNode.GetAttributeValue("href", ""));
                var uri = new Uri(lastPageUrl);
                var queryParams = QueryHelpers.ParseQuery(uri.Query);

                if (queryParams.TryGetValue("page", out var pageValues) &&
                    int.TryParse(pageValues.ToString(), out int pageNumber))
                {
                    maxPage = pageNumber;
                }
            }

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
                var description = await ScrapeNovelDescription(sourceUrl);

                await SaveNovelAndAuthorAsync(title, authorName, sourceUrl, description);
            }
        }

        public async Task<String> ScrapeNovelDescription(string sourceUrl, int retryCount = 0)
        {
            const int maxRetries = 5;

            var html = await _apiClient.Get(sourceUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var descDiv = doc.DocumentNode.SelectSingleNode("//div[@class='desc-text']");
            if (descDiv is null && retryCount < maxRetries)
            {
                await Task.Delay(2000); // Wait for 2 seconds before retrying
                return await ScrapeNovelDescription(sourceUrl, retryCount + 1);
            }

            if (descDiv is not null)
            {
                var description = descDiv.InnerHtml;
                description = Regex.Replace(description, @"\s+", " ");

                // Trim leading and trailing spaces
                description = description.Trim();
                return description;
            }
            return string.Empty;
        }

        ////////////////////////////////////////////////////
        public async Task ScrapeAllTitles()
        {
            var novels = await _novelService.GetAllNovels();
            novels.ToList();
            var filtereed = novels.Where(x => x.NovelId != 1);
            foreach (var novel in filtereed)
            {
                await ScrapeChapterTitleUrl(novel.SourceUrl + ChaptersTitle, novel.NovelId);
            }
        }

        public async Task ScrapeChapterTitleUrl(string chapterTitlesUrl, int novelId)
        {
            var html = await _apiClient.Get(chapterTitlesUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var panelBody = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'panel-body')]");
            if (panelBody == null) return;

            var rowDivs = panelBody.SelectNodes(".//div[contains(@class, 'row')]");
            if (rowDivs == null) return;

            foreach (var row in rowDivs)
            {
                var colDivs = row.SelectNodes(".//div[contains(@class, 'col-xs-12') and contains(@class, 'col-sm-4') and contains(@class, 'col-md-4')]");
                if (colDivs == null) continue;

                foreach (var col in colDivs)
                {
                    var liNodes = col.SelectNodes(".//ul[contains(@class, 'list-chapter')]/li");
                    if (liNodes == null) continue;

                    foreach (var li in liNodes)
                    {
                        var aTag = li.SelectSingleNode(".//a");
                        if (aTag == null) continue;
                        var chapterUrl = aTag.GetAttributeValue("href", "").Trim();
                        var (contentFilepath,chapaterTitle) = await ScrapeChapterContent(chapterUrl);

                        var (chapterNumber, cleanTitle) = ExtractChapterInfo(chapaterTitle);

                        var chapter = new Chapter
                        {
                            Title = cleanTitle,
                            ChapterNumber = decimal.Parse(chapterNumber),
                            NovelId = novelId,
                            FilePath = contentFilepath,
                            SourceUrl = chapterUrl
                        };

                        await _unitOfWork.ChapterRepository.AddAsync(chapter);
                    }
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
                var chapterTitleH4Tag = chrContentDiv.SelectSingleNode(".//h4");
                var chapterTitleH3Tag = chrContentDiv.SelectSingleNode(".//h3");
                string chapterTitle = chapterTitleH4Tag != null ? chapterTitleH4Tag.InnerText.Trim() : (chapterTitleH3Tag != null ? chapterTitleH3Tag.InnerText.Trim() : string.Empty);

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


        ///////////////////////////////////////////////////////////////

        public Task ScrapeAllPages()
        {
            throw new NotImplementedException();
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

        private async Task SaveNovelAndAuthorAsync(string novelTitle, string authorName, string novelSourceUrl, string description)
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

            var parts = fullTitle.Split(new[] { ':' }, 2);

            if (parts.Length == 2)
            {
                var chapterLabel = parts[0].Trim();  // Example: "Chapter 1"
                var chapterNumber = new string(chapterLabel.Where(char.IsDigit).ToArray());  // Extract digits: "1"
                var cleanTitle = parts[1].Trim();  // Extract right part: "Immortal Body, Exquisite Nine Orifices Sword Heart"

                return (chapterNumber, cleanTitle);
            }

            // fallback if format is unexpected
            return (string.Empty, fullTitle.Trim());
        }
    }
}
