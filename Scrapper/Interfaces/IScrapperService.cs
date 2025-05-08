namespace Scrapper.Interfaces
{
    public interface IScrapperService 
    {
        #region Novels
        Task ScrapeAllPages();
        Task ScrapePage(string url, int page);
        Task ScrapeLatestNovels();
        Task ScrapeHotestNovels();
        Task ScrapeCompletedNovels();
        #endregion

        #region Chapters
        Task ScrapeChapterTitleUrl(string chapterTitlesUrl, int novelId);
        Task<string> ScrapeChapterContent(string novelChapterUrl);
        #endregion

    }
}
