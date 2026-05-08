using Microsoft.EntityFrameworkCore;
using TheWell.Data;

namespace TheWell.Tests;

internal static class TestDbContextFactory
{
    public static WellDbContext Create()
    {
        var opts = new DbContextOptionsBuilder<WellDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var db = new WellDbContext(opts);
        db.Database.EnsureCreated();
        return db;
    }
}
