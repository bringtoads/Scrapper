namespace Scrapper.Interfaces
{
    interface IScrapperService 
    {
        #region Novels
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
