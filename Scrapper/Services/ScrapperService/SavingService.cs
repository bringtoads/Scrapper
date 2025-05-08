using Scrapper.Contracts.DTOs;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;
using Scrapper.Interfaces;

namespace Scrapper.Services.ScrapperService
{
    public class SavingService : ISavingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INovelService _novelService;
        private readonly IAuthorService _authorService;

        public SavingService(IUnitOfWork unitOfWork, INovelService novelService, IAuthorService authorService)
        {
            _unitOfWork = unitOfWork;
            _novelService = novelService;
            _authorService = authorService;
        }

        public async Task SaveNovelAndAuthorAsync(ScrapedNovelDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var author = await _authorService.GetByNameAsync(dto.AuthorName);
                if (author == null)
                {
                    author = new Author { Name = dto.AuthorName };
                    await _authorService.AddAsync(author);
                    await _unitOfWork.CompleteAsync();
                }

                var existingNovel = await _novelService.GetBySourceUrlAsync(dto.SourceUrl);
                if (existingNovel == null)
                {
                    var novel = new Novel
                    {
                        Title = dto.Title,
                        SourceUrl = dto.SourceUrl,
                        Description = dto.Description,
                        AuthorId = author.AuthorId
                    };

                    await _novelService.AddAsync(novel);
                    await _unitOfWork.CompleteAsync();
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
