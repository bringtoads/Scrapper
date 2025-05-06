using Scrapper.Data.Repositories;
using Scrapper.Interfaces;

namespace Scrapper.Services
{
    internal class ChapterService : IChapterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INovelService _novelService;
        private string ChapterListNav = "#tab-chapters-title";
        private readonly IScrapperService _scrapper;

        public ChapterService(IUnitOfWork unitOfWork, INovelService novelService,IScrapperService scrapper)
        {
            _unitOfWork = unitOfWork;
            _novelService = novelService;
            _scrapper = scrapper;
        }

        //from database scrape all chapter title url 
        public async Task ScrapeChapterDetailsFromAllNovels()
        {
            var novels = await _novelService.GetAllNovelDetails();
            if (novels is not null)
            {
                foreach (var novel in novels)
                {
                    await ScrapeChapterTitleUrl(novel.SourceUrl + ChapterListNav, novel.NovelId);
                }
            }
        }
      
        public async Task ScrapeChapterTitleUrl(string chapterTitlesUrl,int novelId)
        {
            await _scrapper.ScrapeChapterTitleUrl(chapterTitlesUrl,novelId);
        }

        public async Task<string> ScrapeChapterContent(string novelChapterUrl)
        {
            return await _scrapper.ScrapeChapterContent(novelChapterUrl);
        }

    }
}
