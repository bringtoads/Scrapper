using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Scrapper.Data.Entity;

namespace Scrapper.Data.Configs.EntityConfigurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("tags");

            builder.HasKey(t => t.TagId);
            builder.Property(t => t.TagId).HasColumnName("tag_id").ValueGeneratedOnAdd();

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");
        }
    }
}
