using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheWell.Core.Entities;

namespace TheWell.Data.Configurations;

public class MetadataCacheConfiguration : IEntityTypeConfiguration<MetadataCache>
{
    public void Configure(EntityTypeBuilder<MetadataCache> builder)
    {
        builder.HasKey(m => m.CacheID);
        builder.Property(m => m.ContentType).HasMaxLength(50).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.HasIndex(m => m.ContentType);
    }
}
