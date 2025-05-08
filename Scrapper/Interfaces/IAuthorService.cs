using Scrapper.Data.Entity;

namespace Scrapper.Interfaces
{
    public interface IAuthorService
    {
        Task<Author?> GetByNameAsync(string name);
        Task AddAsync(Author author);

    }
}
