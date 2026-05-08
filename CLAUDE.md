# The Well —  Document

## What This Project Is

**The Well** is a 60-day habit-tracker app built for a university wellness course. Students log in, fill out a commitment plan, then track a daily habit over 8 weeks. An admin portal manages users and course dates. Content (weekly modules, motivational messages) is pulled from a WordPress CMS.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Mobile App | .NET MAUI (Windows/Android/iOS) |
| API Server | ASP.NET Core Web API (.NET 10) |
| Admin Portal | Blazor Server (.NET 10) |
| Database | PostgreSQL via Neon (free tier, cloud-hosted) |
| CMS | WordPress.com staging site |
| Email | SendGrid (transactional + dynamic templates) |
| Auth | JWT Bearer tokens |
| ORM | EF Core 10 + Npgsql |

---

## Solution Structure

```
TheWell.slnx
├── TheWell.Core        — DTOs, entities, interfaces (no dependencies)
├── TheWell.Data        — EF Core DbContext, repositories, migrations
├── TheWell.API         — ASP.NET Core Web API (runs on port 5139)
├── TheWell.Admin       — Blazor Server admin portal
├── TheWell.MAUI        — .NET MAUI mobile app
└── TheWell.Tests       — (empty, future use)
```

---

## Running the Project Locally

1. Copy the example secrets files and fill in your values:
   ```
   cp TheWell.API/appsettings.Development.json.example TheWell.API/appsettings.Development.json
   cp TheWell.Admin/appsettings.Development.json.example TheWell.Admin/appsettings.Development.json
   ```
   Then edit each file with your own DB connection string, JWT secret, SendGrid key, and encryption key.

2. **Start the API** (must run first):
   ```
   cd TheWell.API
   dotnet run
   ```
   Runs on `http://localhost:5139`. On startup it auto-runs EF migrations and applies raw SQL fixes.

3. **Start the Admin** (separate terminal):
   ```
   cd TheWell.Admin
   dotnet run
   ```

4. **Run MAUI** from Visual Studio — target **Windows Machine**.

> On Android emulator the base URL is `http://10.0.2.2:5139/`. On Windows/iOS it is `http://localhost:5139/`. This is handled by `#if ANDROID` in `MauiProgram.cs`.

---

## Configuration (Secrets)

All secrets are stored in `appsettings.Development.json` (git-ignored). Never commit real values.
Use the `.example` files as templates. Required keys:

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnections` | Neon PostgreSQL connection string |
| `Jwt:Secret` | Min 32-char random string for signing JWTs |
| `Jwt:Issuer` | `thewell-api` |
| `Jwt:Audience` | `thewell-maui` |
| `SendGrid:ApiKey` | SendGrid API key (starts with `SG.`) |
| `SendGrid:FromEmail` | Verified sender email address |
| `SendGrid:WelcomeTemplateId` | SendGrid dynamic template ID |
| `Encryption:Key2` | Base64-encoded 32-byte AES key |
| `WordPress:BaseUrl` | Your WordPress site URL |

---

## Database (Neon PostgreSQL)

Connection string goes in `appsettings.Development.json` — see the `.example` file for the format.

**Tables:**
- `Users` — stores encrypted E-number + email, BCrypt password hash, account status
- `IntakeQuestions` — one row per user; stores the 8-field commitment plan answers
- `Goals` — (currently unused in UI, kept for potential future use)
- `DailyLogs` — one row per completed day (only created when `IsCompleted = true`)
- `AuthenticationAudits` — login attempts, OTP requests, password resets
- `CourseConfigs` — admin-set course start/end dates
- `MetadataCache` — caches WordPress API responses for 24 hours
- `WeekLocks` — admin-controlled lock/unlock state per WordPress week

**Migrations** live in `TheWell.Data/Migrations/`. The API calls `db.Database.Migrate()` on startup. There is also a raw SQL block in `Program.cs` that ensures the `IntakeQuestions` table columns and `WeekLocks` table are correct.

---

## Encryption & Security

- **E-number and email** are stored AES-256 encrypted (non-deterministic, for display/decryption)
- **E-number lookup** uses HMAC-SHA256 (`UniversityEIDHash` column) — deterministic, used for DB queries
- **Passwords** use BCrypt (BCrypt.Net-Next 4.1.0)
- **JWT** tokens: 60-minute access tokens, 30-day refresh tokens
- Encryption key and JWT secret are stored in `appsettings.Development.json` (git-ignored)

---

## User Flow (MAUI App)

1. **Login** (`LoginPage`) — user enters E-number + password → `POST /api/auth/login`
2. **Force Reset** (`ForceResetPage`) — if `IsPasswordResetRequired = true`, user must set a new password before proceeding
3. **Intake Form** (`IntakePage`) — one-time commitment plan with 8 questions; submitted once, then read-only
4. **Dashboard** (`DashboardPage`) — home tab showing habit panel, feed cards, 8-week calendar
5. **Habit tab** (`GoalPage`) — read-only view of the commitment plan answers
6. **Course tab** (`CoursePage`) — week list from WordPress; unlocked weeks are readable
7. **Settings tab** (`SettingsPage`) — view E-number/email, change password
8. **Log Entry** (`LogEntryPage`) — tap a calendar day to log it; only saves if `IsCompleted = true`; editable within 5 days of the date

### Bottom Navigation Tabs
- **W** — Well (Dashboard)
- **H** — Habit (commitment plan)
- **C** — Course (WordPress content)
- **S** — Settings

---

## Admin Portal Flow

Navigate to: `http://localhost:[admin-port]/`

**Pages:**
- `/` — Welcome page with portal description
- `/users` — Provision users (auto-sends welcome email via SendGrid), reset passwords (sends email), delete users
- `/course-config` — Set course start date (Monday-only picker); Populate button auto-unlocks weeks; manual per-week lock/unlock
- `/audit` — "Coming soon"
- `/student-stats` — "Coming soon"

---

## API Endpoints

### Auth (`/api/auth`)
| Method | Route | Description |
|---|---|---|
| POST | `/login` | Login with E-number + password |
| POST | `/force-reset` | Set new password (first-login) |
| POST | `/otp/request` | Request OTP code via email |
| POST | `/otp/verify` | Verify OTP → get password reset token |

### Intake (`/api/intake`) — requires JWT
| Method | Route | Description |
|---|---|---|
| GET | `/` | Get current user's intake answers |
| POST | `/` | Submit intake form (one time only) |

### Logs (`/api/logs`) — requires JWT
| Method | Route | Description |
|---|---|---|
| GET | `/` | Get all logs for current user |
| POST | `/` | Create log for a specific date |
| PUT | `/{logId}` | Update existing log |

### Stats (`/api/stats`) — requires JWT
| GET | `/api/stats` | TotalCompleted, CurrentStreak, WellFillPercent |

### Content (`/api/content`) — requires JWT
| Method | Route | Description |
|---|---|---|
| GET | `/current-week` | Fetches current week's WordPress content |
| GET | `/weeks` | All weeks + lock status (locked if no course date set) |
| GET | `/weeks/{n}` | Full content for week N (403 if locked) |
| PUT | `/weeks/{n}/lock` | Admin: toggle lock state |
| POST | `/populate` | Admin: auto-unlock weeks based on today's date |
| DELETE | `/cache` | Clear WordPress content cache |

### Users (`/api/users`) — requires JWT
| GET | `/api/users/me` | Get current user's decrypted E-number + email |
| PUT | `/api/users/me/password` | Change password (requires current password) |

### Config
| GET | `/api/config` | Get course start date (no auth required) |

---

## WordPress CMS

Set `WordPress:BaseUrl` in `appsettings.Development.json`.

**Custom Post Type**: `weekly_content` (REST base: `weekly_content`)

**ACF Fields** (Show in REST API must be ON):
| Field Name | Type | Description |
|---|---|---|
| `week_number` | Number | Which week (1–8) |
| `module_number` | Number | Module number |
| `module_title` | Text | e.g. "Introduction to Habit Formation" |
| `motivational_message` | Text Area | Weekly motivational message |
| `course_material` | Wysiwyg | HTML content, renders in WebView in app |
| `notifications` | Text | Notification message text |
| `day_1-7` | Number | Day of week (1–7) to show the notification |

---

## Email (SendGrid)

Configured via `appsettings.Development.json`. Required fields: `ApiKey`, `FromEmail`, `WelcomeTemplateId`.

**Template dynamic variables** (must exist in the SendGrid template):
- `{{e_number}}` — the user's E-number
- `{{temp_password}}` — the temporary password

**When emails are sent:**
- New user provisioned → welcome email with E-number + temp password
- Password reset → same welcome email with new temp password
- OTP requested → plain HTML email with 6-digit code

---

## Key Constraints & Business Logic

### Calendar
- 8 weeks = 56 days starting from `CourseConfig.CourseStartDate`
- Start date **must be a Monday** (enforced in admin, browser date picker uses `step=7`)
- End date = start + 55 days (always a Sunday)
- "Day X of 60" = current date relative to course start (not completed count)
- Days are colour-coded: Orange=Today, Teal=Completed, Pink=Editable (< 5 days), Grey=Future/Locked
- Users can only edit logs within **5 days** of the actual date
- A log entry is only created in the DB if `IsCompleted = true`

### Week Locking
- All weeks default to locked
- If no course start date is set, all weeks are forced locked regardless of DB state
- Admin uses Populate to auto-unlock weeks whose start date has passed
- Admin can manually override any week's lock state

### Account Statuses
- `Pending` — provisioned by admin, temp password not yet changed
- `Active` — user has completed ForceReset (set their own password)
- `Suspended` — login blocked
- `Graduation` — course ended, redirected to GraduationPage

---

## Known Issues & Technical Debt

1. **EF Migration discovery** — raw SQL in `Program.cs` ensures schema is correct on startup as a workaround.
2. **Goals feature** — entity/controller exist but UI doesn't use them. Can be removed.
3. **MetadataCache** — 24-hour TTL; no manual cache-bust in UI (use `DELETE /api/content/cache`).
4. **WordPressContentService** — registered in MAUI DI but unused. Can be removed.
5. **Admin auth** — the admin portal has no login; anyone with the URL can access it.
6. **Refresh tokens** — stored but refresh endpoint not implemented.

---

## How to Add a New Feature (Checklist)

1. Add entity to `TheWell.Core/Entities/` if needed
2. Add DTO(s) to `TheWell.Core/DTOs/`
3. Add EF configuration to `TheWell.Data/Configurations/`
4. Register DbSet in `WellDbContext.cs`
5. Create repository in `TheWell.Data/Repositories/`
6. Run `dotnet ef migrations add MigrationName --project TheWell.Data --startup-project TheWell.API`
7. Register repository in `TheWell.API/Program.cs`
8. Add API controller
9. Add method to `TheWell.MAUI/Services/ApiService.cs`
10. Create ViewModel + Page in MAUI
11. Register in `MauiProgram.cs`
12. Register route in `AppShell.xaml.cs` if it's a push route (not a tab)
