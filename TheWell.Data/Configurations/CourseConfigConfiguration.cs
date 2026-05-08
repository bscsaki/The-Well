using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheWell.Core.Entities;

namespace TheWell.Data.Configurations;

public class CourseConfigConfiguration : IEntityTypeConfiguration<CourseConfig>
{
    public void Configure(EntityTypeBuilder<CourseConfig> builder)
    {
        builder.HasKey(c => c.ConfigID);
    }
}
