using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheWell.Core.Entities;

namespace TheWell.Data.Configurations;

public class DailyLogConfiguration : IEntityTypeConfiguration<DailyLog>
{
    public void Configure(EntityTypeBuilder<DailyLog> builder)
    {
        builder.HasKey(l => l.LogID);
        builder.HasIndex(l => new { l.UserID, l.LogDate }).IsUnique();
        builder.Property(l => l.IsLocked);
    }
}
