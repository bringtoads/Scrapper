using Scrapper.Data.Entity;

namespace Scrapper.Interfaces
{
    interface INovelService
    {
        Task<IEnumerable<Novel>> GetAllNovelDetails();
        Task ScrapeLatest();
        Task<String> ScrapeNovelDescription(string sourceUrl); 
    }
}
