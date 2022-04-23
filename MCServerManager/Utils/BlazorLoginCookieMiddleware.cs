using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using MCServerManager.Models;

namespace MCServerManager.Utils;

public class BlazorCookieLoginMiddleware
{
    private static readonly IDictionary<Guid, UserInputAuthModel> Logins = new ConcurrentDictionary<Guid, UserInputAuthModel>();

    public static Guid AnnounceLogin(UserInputAuthModel loginInfo)
    {
        var key = Guid.NewGuid();
        Logins[key] = loginInfo;
        return key;
    }

    public static UserInputAuthModel GetLoginInProgress(string key)
    {
        return GetLoginInProgress(Guid.Parse(key));
    }

    public static UserInputAuthModel GetLoginInProgress(Guid key)
    {
        if (Logins.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }

    private readonly RequestDelegate _next;

    public BlazorCookieLoginMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        if (context.Request.Path == "/" && context.Request.Query.ContainsKey("key"))
        {
            var key = Guid.Parse(context.Request.Query["key"]);
            var info = Logins[key];

            var user = await userManager.FindByEmailAsync(info.Email);
            var result = await signInManager.PasswordSignInAsync(user, info.Password, false, false);

            // Uncache password for security
            info.Password = null;

            if (result.Succeeded)
            {
                Logins.Remove(key);
                context.Response.Redirect("/home");
            }
        }
        
        if (context.Request.Path.StartsWithSegments("/logout"))
        {
            await signInManager.SignOutAsync();
            context.Response.Redirect("/");
        }

        // Continue http middleware chain:
        await _next.Invoke(context);
    }
}
