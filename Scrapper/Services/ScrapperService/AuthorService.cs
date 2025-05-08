using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;
using Scrapper.Interfaces;

namespace Scrapper.Services.ScrapperService
{
    public class AuthorService : IAuthorService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AuthorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddAsync(Author author)
        {
            await _unitOfWork.AuthorRepository.AddAsync(author);
        }
        public async Task UpdateAuthor(Author author)
        {
            await _unitOfWork.AuthorRepository.UpdateAsync(author);
        }
        public async Task<Author?> GetByNameAsync(string name)
        {
            return await _unitOfWork.AuthorRepository.GetByNameAsync(name);
        }
    }
}
