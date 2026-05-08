using TheWell.Data.Repositories;

namespace TheWell.API.Middleware;

public class GraduationGuardMiddleware(RequestDelegate next)
{
    private static readonly string[] WriteVerbs = ["POST", "PUT", "PATCH", "DELETE"];
    private static readonly string[] GuardedPaths = ["/api/logs", "/api/goals"];

    public async Task InvokeAsync(HttpContext context, CourseConfigRepository configRepo)
    {
        if (WriteVerbs.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase) &&
            GuardedPaths.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
        {
            var config = await configRepo.GetCurrentAsync();
            if (config is not null && DateOnly.FromDateTime(DateTime.UtcNow) > config.CourseEndDate)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Course has ended. Content is read-only." });
                return;
            }
        }
        await next(context);
    }
}
