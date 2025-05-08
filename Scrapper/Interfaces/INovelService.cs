using Scrapper.Data.Entity;

namespace Scrapper.Interfaces
{
    public interface INovelService
    {
        Task<IEnumerable<Novel>> GetAllNovels();
        Task GetNovel();
        Task AddAsync(Novel novel);
        Task<Novel?> GetBySourceUrlAsync(string source);
    }
}
