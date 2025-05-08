using Scrapper.Contracts.DTOs;

namespace Scrapper.Interfaces
{
    public interface ISavingService
    {
        Task SaveNovelAndAuthorAsync(ScrapedNovelDto dto);
    }
}
