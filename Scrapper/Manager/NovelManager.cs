
using Scrapper.Data.Entity;
using Scrapper.Interfaces;

namespace Scrapper.Manager
{
    internal class NovelManager : INovelManager
    {
        private readonly IScrapperService _scrapperService;
        private readonly INovelService _novelSerice;
        private readonly IChapterService _chatperService;
        public NovelManager(IScrapperService scrapperService, IChapterService chapterService)
        {
            _scrapperService = scrapperService;
            _chatperService = chapterService;
        }
        public async Task Start()
        {
            var novels =await _novelSerice.GetAllNovelDetails();
            if (novels is null)
            {
                
            }
        }
    }
}
