using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheWell.Core.Entities;

namespace TheWell.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserID);
        builder.Property(u => u.UniversityEID).HasMaxLength(500).IsRequired();
        builder.HasIndex(u => u.UniversityEID).IsUnique();
        builder.Property(u => u.UniversityEIDHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(u => u.UniversityEIDHash);
        builder.Property(u => u.Email).HasMaxLength(500).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.AccountStatus).HasMaxLength(20);
        builder.Property(u => u.IsPasswordResetRequired);

        builder.HasOne(u => u.IntakeQuestion)
               .WithOne(i => i.User)
               .HasForeignKey<IntakeQuestion>(i => i.UserID)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Goal)
               .WithOne(g => g.User)
               .HasForeignKey<Goal>(g => g.UserID)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.DailyLogs)
               .WithOne(l => l.User)
               .HasForeignKey(l => l.UserID)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AuthAudits)
               .WithOne(a => a.User)
               .HasForeignKey(a => a.UserID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
