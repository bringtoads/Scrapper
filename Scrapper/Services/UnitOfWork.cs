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

        /// <summary>
        /// This method starts a new database transaction by calling BeginTransactionAsync() on the DbContext.Database property.
        ///It stores the IDbContextTransaction object in a private _transaction field to keep track of the active transaction.
        /// </summary>
        /// <returns></returns>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// This method saves changes to the database via SaveChangesAsync() and commits the transaction.
        ///If any exception occurs during SaveChangesAsync(), it rolls back the transaction and rethrows the exception to maintain consistency.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// This method rolls back the transaction if any error occurs or if you want to discard all changes made during the transaction.
        /// </summary>
        /// <returns></returns>
        public async Task RollbackAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();
            }
        }

        /// <summary>
        /// This is the standard method to save changes. In this case, it saves changes and can be used outside of a transaction context if you’re not using transactions for a particular operation.
        /// </summary>
        /// <returns></returns>
        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// This is part of the disposable pattern used for cleaning up unmanaged resources or other resources that require explicit cleanup when an object is no longer needed
        /// </summary>
        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
