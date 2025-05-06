using Scrapper.Data.Entity;

namespace Scrapper.Data.Repositories
{
    public interface IChapterRepository
    {
        Task<Chapter?> GetByIdAsync(int id);
        Task<IEnumerable<Chapter>> GetAllAsync();
        Task AddAsync(Chapter chapter);
        Task UpdateAsync(Chapter chapter);
        Task DeleteAsync(int id);
    }
}
