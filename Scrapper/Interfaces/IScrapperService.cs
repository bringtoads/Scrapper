namespace Scrapper.Interfaces
{
    interface IScrapperService 
    {
        Task ScrapeChapterTitleUrl(string chapterTitlesUrl, int novelId);
        Task<string> ScrapeChapterContent(string novelChapterUrl);
    }
}
