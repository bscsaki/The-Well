using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheWell.Core.Entities;

namespace TheWell.Data.Configurations;

public class WeekLockConfiguration : IEntityTypeConfiguration<WeekLock>
{
    public void Configure(EntityTypeBuilder<WeekLock> builder)
    {
        builder.HasKey(w => w.WeekNumber);
        builder.Property(w => w.WeekNumber).ValueGeneratedNever();
        builder.Property(w => w.IsLocked).IsRequired().HasDefaultValue(true);
    }
}
