namespace Scrapper.Data.Entity
{
    public class Author
    {
        public int AuthorId { get; set; }
        public string Name { get; set; } = null!;
        public string? Biography { get; set; }
        public string? Website { get; set; }

        // Navigation properties
        public virtual ICollection<Novel> Novels { get; set; } = new List<Novel>();
    }
}
