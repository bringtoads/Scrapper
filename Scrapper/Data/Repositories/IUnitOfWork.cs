namespace Scrapper.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IAuthorRepository AuthorRepository { get; }
        INovelRepository NovelRepository { get; }
        IChapterRepository ChapterRepository { get; }
        //IScrapeHistoryRepository ScrapeHistoryRepository { get; }
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
