using Scrapper.Data.Entity;

namespace Scrapper.Data.Repositories
{
    public interface IChapterRepository
    {
        Task<Chapter?> GetByIdAsync(int id);
        Task<IEnumerable<Chapter>> GetAllAsync();
        Task<List<Chapter>> GetChaptersByNovelIdAsync(int novelId);
        Task AddAsync(Chapter chapter);
        Task UpdateAsync(Chapter chapter);
        Task DeleteAsync(int id);
    }
}
