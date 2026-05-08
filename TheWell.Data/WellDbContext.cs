using Microsoft.EntityFrameworkCore;
using TheWell.Core.Entities;

namespace TheWell.Data;

public class WellDbContext(DbContextOptions<WellDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<IntakeQuestion> IntakeQuestions => Set<IntakeQuestion>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<DailyLog> DailyLogs => Set<DailyLog>();
    public DbSet<MetadataCache> MetadataCache => Set<MetadataCache>();
    public DbSet<AuthenticationAudit> AuthenticationAudits => Set<AuthenticationAudit>();
    public DbSet<CourseConfig> CourseConfigs => Set<CourseConfig>();
    public DbSet<WeekLock> WeekLocks => Set<WeekLock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WellDbContext).Assembly);
    }
}
