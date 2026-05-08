using Microsoft.EntityFrameworkCore;
using SendGrid.Extensions.DependencyInjection;
using TheWell.Admin.Components;
using TheWell.Core.Interfaces;
using TheWell.API.Services;
using TheWell.Data;
using TheWell.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WellDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnections")));

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<CourseConfigRepository>();
builder.Services.AddScoped<WeekLockRepository>();
builder.Services.AddScoped<AuditRepository>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddSendGrid(opt =>
    opt.ApiKey = builder.Configuration["SendGrid:ApiKey"] ?? "");
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Ensure WeekLocks table exists (API also creates it, but Admin may run independently)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WellDbContext>();
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ""WeekLocks"" (
            ""WeekNumber"" integer NOT NULL,
            ""IsLocked"" boolean NOT NULL DEFAULT true,
            CONSTRAINT ""PK_WeekLocks"" PRIMARY KEY (""WeekNumber"")
        );
    ");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
