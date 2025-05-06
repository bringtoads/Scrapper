using Microsoft.EntityFrameworkCore;
using Scrapper.Data.Configs.EntityConfigurations;
using Scrapper.Data.Entity;

namespace Scrapper.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Novel> Novels { get; set; } = null!;
        public DbSet<Chapter> Chapters { get; set; } = null!;
        public DbSet<ScrapeHistory> ScrapeHistory { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AuthorConfiguration());
            modelBuilder.ApplyConfiguration(new NovelConfiguration());
            modelBuilder.ApplyConfiguration(new ChapterConfiguration());
            modelBuilder.ApplyConfiguration(new ScrapeHistoryConfiguration());
        }
    }
}
