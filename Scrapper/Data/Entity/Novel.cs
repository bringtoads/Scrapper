namespace Scrapper.Data.Entity
{
    public class Novel
    {
        public int NovelId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public string SourceUrl { get; set; } = null!;
        public string? Status { get; set; } // ongoing, completed, hiatus
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public int? AuthorId { get; set; }
        public virtual Author? Author { get; set; } = null!;
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
      
        public virtual ICollection<ScrapeHistory> ScrapeHistory { get; set; } = new List<ScrapeHistory>();
    }
}
