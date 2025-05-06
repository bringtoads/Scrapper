using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Scrapper.Data.Entity;

namespace Scrapper.Data.Configs.EntityConfigurations
{
    public class NovelConfiguration : IEntityTypeConfiguration<Novel>
    {
        public void Configure(EntityTypeBuilder<Novel> builder)
        {
            builder.ToTable("novels");

            builder.HasKey(n => n.NovelId);

            builder.Property(a => a.NovelId).HasColumnName("novel_id");

            builder.Property(c => c.NovelId)
               .ValueGeneratedOnAdd();

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("title");

            builder.Property(n => n.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");

            builder.Property(n => n.CoverUrl)
                .HasMaxLength(500)
                .HasColumnName("cover_url");

            builder.Property(n => n.AuthorId)
            .HasColumnName("author_id");

            builder.Property(n => n.SourceUrl)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("source_url"); 

            builder.Property(n => n.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            builder.Property(n => n.CreatedAt)
                .HasDefaultValueSql("GETDATE()")
                .HasColumnName("created_at"); 

            builder.Property(n => n.UpdatedAt)
                .HasDefaultValueSql("GETDATE()")
                .HasColumnName("updated_at");

            builder.HasOne(n => n.Author)
                .WithMany(a => a.Novels)
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(n => n.Chapters)
                .WithOne(c => c.Novel)
                .HasForeignKey(c => c.NovelId)
                .OnDelete(DeleteBehavior.Cascade); // When Novel is deleted, its Chapters go too

            builder.HasMany(n => n.ScrapeHistory)
                .WithOne(sh => sh.Novel)
                .HasForeignKey(sh => sh.NovelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
