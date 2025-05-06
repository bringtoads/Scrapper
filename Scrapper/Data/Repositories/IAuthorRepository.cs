using Scrapper.Data.Entity;

namespace Scrapper.Data.Repositories
{
    public interface IAuthorRepository
    {
        Task<Author?> GetByIdAsync(int id);
        Task<Author?> GetByNameAsync(string authorName);
        Task<IEnumerable<Author>> GetAllAsync();
        Task AddAsync(Author author);
        Task UpdateAsync(Author author);
        Task DeleteAsync(int id);
    }
}
