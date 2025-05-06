using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Scrapper.Data.Entity;

namespace Scrapper.Data.Configs.EntityConfigurations
{
    public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
    {
        public void Configure(EntityTypeBuilder<Chapter> builder)
        {
            builder.ToTable("chapters");

            builder.HasKey(c => c.ChapterId);
            builder.Property(a => a.ChapterId).HasColumnName("chapter_id");

            builder.Property(c => c.ChapterId)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.ChapterNumber)
                .IsRequired()
                .HasColumnType("decimal(5,2)")
                .HasColumnName("chapter_number");

            builder.Property(c => c.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            builder.Property(c => c.FilePath)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("file_path");

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETDATE()")
                .HasColumnName("created_at");

            builder.Property(c => c.SourceUrl)
                .HasMaxLength(500)
                .HasColumnName("source_url");

            builder.HasOne(c => c.Novel)
                .WithMany(n => n.Chapters)
                .HasForeignKey(c => c.NovelId)
                .OnDelete(DeleteBehavior.Cascade); // Chapter depends on Novel

        }
    }
}
