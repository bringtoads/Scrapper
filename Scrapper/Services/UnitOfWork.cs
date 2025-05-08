using Microsoft.EntityFrameworkCore.Storage;
using Scrapper.Data;
using Scrapper.Data.Repositories;

namespace Scrapper.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction _transaction; 

        public IAuthorRepository AuthorRepository { get; }
        public INovelRepository NovelRepository { get; }
        public IChapterRepository ChapterRepository { get; }
        //public IScrapeHistoryRepository ScrapeHistoryRepo { get; }

        public UnitOfWork(AppDbContext dbContext,
                          IAuthorRepository authorRepo,
                          INovelRepository novelRepo,
                          IChapterRepository chapterRepo)
        {
            _dbContext = dbContext;
            AuthorRepository = authorRepo;
            NovelRepository = novelRepo;
            ChapterRepository = chapterRepo;
            //ScrapeHistoryRepo = scrapeHistoryRepo;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw; // Rethrow exception after rollback
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();
            }
        }

        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
