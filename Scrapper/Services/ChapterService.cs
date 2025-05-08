using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;
using Scrapper.Interfaces;
using Scrapper.Repositories;

namespace Scrapper.Services
{
    internal class ChapterService : IChapterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INovelService _novelService;

        public ChapterService(IUnitOfWork unitOfWork, INovelService novelService)
        {
            _unitOfWork = unitOfWork;
            _novelService = novelService;
        }

        public Task GetAllChapters()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Chapter>> GetAllChaptersByNovelAsync(int novelId)
        {
            return await _unitOfWork.ChapterRepository.GetChaptersByNovelIdAsync(novelId);
        }
        public Task GetChapter(int novelId, int chapterId)
        {
            throw new NotImplementedException();
        }

        public Task SaveChapter()
        {
            throw new NotImplementedException();
        }
    }
}
