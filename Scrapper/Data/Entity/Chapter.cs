namespace Scrapper.Data.Entity
{
    public class Chapter
    {
        public int ChapterId { get; set; }
        public decimal ChapterNumber { get; set; }
        public string? Title { get; set; }
        public string FilePath { get; set; } = null!;
        public int? WordCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? SourceUrl { get; set; }
        // Navigation properties
        public int NovelId { get; set; }
        public virtual Novel Novel { get; set; } = null!;

    }
}
