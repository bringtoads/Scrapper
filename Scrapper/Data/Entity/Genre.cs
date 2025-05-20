namespace Scrapper.Data.Entity
{
    public class Genre
    {
        public int GenreId { get; set; }
        public string Name { get; set; }

        public ICollection<Novel> Novels { get; set; }
    }
}
