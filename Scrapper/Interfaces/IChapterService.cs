using Scrapper.Data.Entity;

namespace Scrapper.Interfaces
{
    public interface IChapterService 
    {
        Task GetAllChapters();
        Task<List<Chapter>> GetAllChaptersByNovelAsync(int novelId);
        Task GetChapter(int novelId, int chapterId);
        Task SaveChapter();
        
    }
}
