using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Scrapper.Data.Entity;

namespace Scrapper.Data.Configs.EntityConfigurations
{
    public class GenreConfiguration : IEntityTypeConfiguration<Genre>
    {
        public void Configure(EntityTypeBuilder<Genre> builder)
        {
            builder.ToTable("genres");

            builder.HasKey(g => g.GenreId);
            builder.Property(g => g.GenreId).HasColumnName("genre_id").ValueGeneratedOnAdd();

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");
        }
    }
}
