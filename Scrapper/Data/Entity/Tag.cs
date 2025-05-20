namespace Scrapper.Data.Entity
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; }

        public ICollection<Novel> Novels { get; set; }
    }
}
