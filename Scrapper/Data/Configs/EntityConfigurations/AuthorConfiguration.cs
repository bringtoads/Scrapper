using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Scrapper.Data.Entity;

namespace Scrapper.Data.Configs.EntityConfigurations
{
    public class AuthorConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.ToTable("authors");

            builder.HasKey(a => a.AuthorId);
            builder.Property(a => a.AuthorId).HasColumnName("author_id");

            builder.Property(c => c.AuthorId)
           .ValueGeneratedOnAdd();

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("name");

            builder.Property(a => a.Biography)
                .HasMaxLength(1000)
                .HasColumnName("biography");

            builder.Property(a => a.Website)
                .HasMaxLength(255)
                .HasColumnName("website");

            builder.HasMany(a => a.Novels)
                .WithOne(n => n.Author)
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
