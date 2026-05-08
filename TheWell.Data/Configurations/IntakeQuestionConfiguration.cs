using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheWell.Core.Entities;

namespace TheWell.Data.Configurations;

public class IntakeQuestionConfiguration : IEntityTypeConfiguration<IntakeQuestion>
{
    public void Configure(EntityTypeBuilder<IntakeQuestion> builder)
    {
        builder.HasKey(i => i.UserID);
    }
}
