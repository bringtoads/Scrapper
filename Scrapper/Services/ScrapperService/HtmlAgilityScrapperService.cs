using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Scrapper.Data.Configs;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;
using Scrapper.Helper;
using Scrapper.Interfaces;
using System.Net;

namespace Scrapper.Services.ScrapperService
{
    public class HtmlAgilityScrapperService 
    {
        private readonly NovelSettings _settings;
        private readonly INovelService _novelService;
        private readonly IChapterService _chapterService;
        private readonly NovelApiClient _apiClient;
        private readonly IUnitOfWork _unitOfWork;

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
            foreach(var url in urls)
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
                await ScrapeRows(i,url);
            }
        }

        private async Task ScrapeRows(int pageNum,string url)
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
        public async Task<String> ScrapeNovelDescription(string sourceUrl)
        {
            var html = await _apiClient.Get(sourceUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var descDiv = doc.DocumentNode.SelectSingleNode("//div[@class='desc-text']");
            if (descDiv is not null)
            {
                var description = descDiv.InnerHtml;
                return description;
            }
            return string.Empty;
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
                else
                {
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
            }
        }
        ////////////////////////////////////////////////////
        public async Task ScrapeChapterTitleUrl(string chapterTitlesUrl, int novelId)
        {
            var html = await _apiClient.Get(chapterTitlesUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var chapterListDiv = doc.DocumentNode.SelectSingleNode("//div[@id='tab-chapters']//div[@id='list-chapter']");

            if (chapterListDiv != null)
            {
                var liNodes = chapterListDiv.SelectNodes(".//ul[@class='list-chapter']/li");

                if (liNodes != null)
                {
                    foreach (var li in liNodes)
                    {
                        var aTag = li.SelectSingleNode(".//a");
                        var title = aTag.GetAttributeValue("title", "").Trim();
                        var filepath = aTag.GetAttributeValue("href", "").Trim();
                        var contentFilepath = await ScrapeChapterContent(filepath);
                        var chapter = new Chapter
                        {
                            Title = title,
                            NovelId = novelId,
                            FilePath = contentFilepath,
                            SourceUrl = filepath
                        };
                        await _unitOfWork.ChapterRepository.AddAsync(chapter);
                        Console.WriteLine($"{title}: {contentFilepath}");
                    }
                }
            }
        }
        public async Task<string> ScrapeChapterContent(string novelChapterUrl)
        {
            //var novelChapterUrl1 = "https://novelbin.com/b/skill-hunter-kill-monsters-acquire-skills-ascend-to-the-highest-rank/1-dark-place";
            var html = await _apiClient.Get(novelChapterUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove the div with class 'unlock-buttons text-center' to avoid its <p> tags
            var unlockButtonsDiv = doc.DocumentNode.SelectSingleNode("//div[@class='unlock-buttons text-center']");
            unlockButtonsDiv?.Remove();

            // Now get the #chr-content div
            var chrContentDiv = doc.DocumentNode.SelectSingleNode("//*[@id='chr-content']");

            if (chrContentDiv is not null)
            {
                var pTags = chrContentDiv.Descendants("p");
                string content = "";

                foreach (var p in pTags)
                {
                    // Skip the <p> tag inside the unlock-buttons div, if it exists (already removed above)
                    content += p.InnerText.Trim() + Environment.NewLine + Environment.NewLine;
                }

                // Parse novelName and chapterName from the URL
                var urlParts = novelChapterUrl.Split('/');
                string novelNamee = urlParts[4];      // skill-hunter-kill-monsters-acquire-skills-ascend-to-the-highest-rank
                string chapterName = urlParts[5];    // 1-dark-place

                // Use your NovelSaver here
                var filepath = NovelSaver.SaveChapter(novelNamee, chapterName, content);
                return filepath;
            }
            else
            {
                return string.Empty;
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
    }
}
