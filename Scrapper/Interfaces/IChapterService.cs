namespace Scrapper.Interfaces
{
    interface IChapterService 
    {
        Task ScrapeChapterDetailsFromAllNovels();
        /// <summary>
        /// https://novelbin.com/b/hard-enough#tab-chapters-title
        /// example of the url to be passed
        /// </summary>
        /// <param name="chapterTitleUrl"></param>
        /// <returns></returns>
        Task ScrapeChapterTitleUrl(string chapterTitlesUrl,int novelId);
        /// <summary>
        /// https://novelbin.com/b/skill-hunter-kill-monsters-acquire-skills-ascend-to-the-highest-rank/1-dark-place
        /// </summary>
        /// <param name="chapterUrl"></param>
        /// <returns></returns>
        Task<string> ScrapeChapterContent(string chapterUrl);
    }
}
