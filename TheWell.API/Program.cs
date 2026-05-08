using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Extensions.DependencyInjection;
using TheWell.API.Middleware;
using TheWell.API.Services;
using TheWell.Core.Interfaces;
using TheWell.Data;
using TheWell.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WellDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnections")));

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<GoalRepository>();
builder.Services.AddScoped<DailyLogRepository>();
builder.Services.AddScoped<IntakeRepository>();
builder.Services.AddScoped<AuditRepository>();
builder.Services.AddScoped<MetadataCacheRepository>();
builder.Services.AddScoped<CourseConfigRepository>();
builder.Services.AddScoped<WeekLockRepository>();

builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILockRuleService, LockRuleService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddHttpClient<ContentService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(8);
});

builder.Services.AddSendGrid(opt =>
    opt.ApiKey = builder.Configuration["SendGrid:ApiKey"]
        ?? throw new InvalidOperationException("SendGrid:ApiKey not configured"));

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WellDbContext>();
    db.Database.Migrate();

    // Ensure WeekLocks table exists
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ""WeekLocks"" (
            ""WeekNumber"" integer NOT NULL,
            ""IsLocked"" boolean NOT NULL DEFAULT true,
            CONSTRAINT ""PK_WeekLocks"" PRIMARY KEY (""WeekNumber"")
        );
    ");

    // Ensure IntakeQuestions schema is current regardless of migration state
    db.Database.ExecuteSqlRaw(@"
        ALTER TABLE ""IntakeQuestions""
            DROP COLUMN IF EXISTS ""Q1_Response"",
            DROP COLUMN IF EXISTS ""Q2_Response"",
            DROP COLUMN IF EXISTS ""Q3_Response"";

        ALTER TABLE ""IntakeQuestions""
            ADD COLUMN IF NOT EXISTS ""MyHabit""                text NOT NULL DEFAULT '',
            ADD COLUMN IF NOT EXISTS ""MyGoal""                 text NOT NULL DEFAULT '',
            ADD COLUMN IF NOT EXISTS ""IAmPersonWho""           text NOT NULL DEFAULT '',
            ADD COLUMN IF NOT EXISTS ""Strategy1""              text NOT NULL DEFAULT '',
            ADD COLUMN IF NOT EXISTS ""Strategy2""              text NOT NULL DEFAULT '',
            ADD COLUMN IF NOT EXISTS ""ToImproveMyselfIWill""   text NOT NULL DEFAULT '',
            ADD COLUMN IF NOT EXISTS ""RewardMyselfWith""       text NOT NULL DEFAULT '',
            ADD COLUMN IF NOT EXISTS ""PeopleForEncouragement"" text NOT NULL DEFAULT '';
    ");
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GraduationGuardMiddleware>();
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { status = "TheWell API is running", docs = "/openapi/v1.json" }));

app.Run();
