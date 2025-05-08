using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scrapper.Data.Configs;
using Scrapper.Data.Entity;
using Scrapper.Data.Repositories;
using Scrapper.Helper;
using Scrapper.Interfaces;

namespace Scrapper.Services
{
    public class NovelService : INovelService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NovelService(IUnitOfWork unitOfWork, IOptions<NovelSettings> options, ILogger<NovelService> logger)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddAsync(Novel novel)
        {
            await _unitOfWork.NovelRepository.AddAsync(novel);
        }

        public async Task<IEnumerable<Novel>> GetAllNovels()
        {
            return await _unitOfWork.NovelRepository.GetAllAsync();
        }

        public Task<Novel?> GetBySourceUrlAsync(string source)
        {
            throw new NotImplementedException();
        }

        public async Task GetNovel()
        {
            throw new NotImplementedException();
        }

    }
}

