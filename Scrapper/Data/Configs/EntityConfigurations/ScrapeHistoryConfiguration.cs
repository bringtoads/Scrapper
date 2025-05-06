using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Scrapper.Data.Entity;

namespace Scrapper.Data.Configs.EntityConfigurations
{
    public class ScrapeHistoryConfiguration : IEntityTypeConfiguration<ScrapeHistory>
    {
        public void Configure(EntityTypeBuilder<ScrapeHistory> builder)
        {
            builder.ToTable("scrape_histories");

            builder.HasKey(sh => sh.ScrapeId);

            builder.Property(c => c.ScrapeId)
                .ValueGeneratedOnAdd()
                .HasColumnName("scrape_id");

            builder.Property(sh => sh.LastScrapedAt)
                .HasDefaultValueSql("GETDATE()")
                .HasColumnName("last_scraped_at");

            builder.Property(sh => sh.ChaptersAdded)
                .HasDefaultValue(0)
                .HasColumnName("chapters_added");

            builder.Property(sh => sh.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            builder.HasOne(sh => sh.Novel)
                .WithMany(n => n.ScrapeHistory)
                .HasForeignKey(sh => sh.NovelId)
                .OnDelete(DeleteBehavior.Cascade); // Remove scrape log when novel is deleted
        }
    }
}
