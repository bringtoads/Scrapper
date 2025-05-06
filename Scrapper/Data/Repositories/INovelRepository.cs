using Microsoft.EntityFrameworkCore;
using Scrapper.Data.Entity;

namespace Scrapper.Data.Repositories
{
    public interface INovelRepository
    {
        Task<Novel?> GetByIdAsync(int id);
        Task<Novel?> GetBySourceUrlAsync(string sourceUrl);
        Task<IEnumerable<Novel>> GetAllAsync();
        Task AddAsync(Novel novel);
        Task UpdateAsync(Novel novel);
        Task DeleteAsync(int id);
    }
}
