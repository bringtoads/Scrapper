namespace Scrapper.Data.Entity
{
    public class ScrapeHistory
    {
        public int ScrapeId { get; set; }
        public int NovelId { get; set; }
        public DateTime LastScrapedAt { get; set; }
        public int ChaptersAdded { get; set; } = 0;
        public string? Status { get; set; } // success, failed, partial

        // Navigation properties
        public virtual Novel Novel { get; set; } = null!;
    }
}
