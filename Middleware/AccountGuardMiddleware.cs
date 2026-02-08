using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using task.Data;
using task.Models;

namespace task.Middleware
{
    public class AccountGuardMiddleware
    {
        private readonly RequestDelegate _next;

        public AccountGuardMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {   
            var path = context.Request.Path.Value?.ToLower() ?? "";

            //to make the pages publicly accessible without authentication.
            if (path.StartsWith("/account/login") ||
                path.StartsWith("/account/register") ||
                path.StartsWith("/account/verifyemail") ||
                path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/lib") ||
                path.StartsWith("/favicon") ||
                path.StartsWith("/images"))
            {
                await _next(context);
                return;
            }

            // If not logged in then redirect to login
            if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                context.Response.Redirect("/Account/Login");
                return;
            }

            // Get logged in user id from cookie claims
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                // Bad cookie, sign out and redirect
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/Account/Login");
                return;
            }

            // Check user still exists and not blocked
            var user = await db.Users.FindAsync(userId);

            if (user == null || user.Status == task.Models.User.UserStatus.Blocked)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/Account/Login");
                return;
            }

            await _next(context);
        }
    }
}