using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheWell.Core.Entities;

namespace TheWell.Data.Configurations;

public class AuthenticationAuditConfiguration : IEntityTypeConfiguration<AuthenticationAudit>
{
    public void Configure(EntityTypeBuilder<AuthenticationAudit> builder)
    {
        builder.HasKey(a => a.AuditID);
        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
        builder.Property(a => a.OtpHash).HasMaxLength(255);
    }
}
