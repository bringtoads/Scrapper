namespace Scrapper.Contracts.DTOs
{
    public class ScrapedNovelDto
    {
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
