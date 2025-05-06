using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Scrapper.Data.Configs;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;
using Scrapper.Helper;
using Scrapper.Interfaces;

namespace Scrapper.Services.ScrapperService
{
    internal class HtmlAgilityScrapperService : IScrapperService
    {
        private readonly NovelSettings _settings;
        private readonly INovelService _novelService;
        private readonly IChapterService _chapterService;
        private readonly NovelApiClient _apiClient;
        private readonly IUnitOfWork _unitOfWork;

        public HtmlAgilityScrapperService(IOptions<NovelSettings> options, INovelService novelService, IChapterService chapterSerivce,NovelApiClient apiClient,IUnitOfWork unitOfWork)
        {
            _settings = options.Value;
            _novelService = novelService;
            _chapterService = chapterSerivce;
            _apiClient = apiClient;
            _unitOfWork = unitOfWork;
        }

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

    }
}
