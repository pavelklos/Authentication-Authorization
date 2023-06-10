using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

// *****************************************************************************
const string AuthScheme = "cookie";
const string AuthScheme2 = "cookie2";

// *****************************************************************************
var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthentication(AuthScheme)      // [MICROSOFT AUTHENTICATION MIDDLEWARE]
    .AddCookie(AuthScheme)              // [MICROSOFT AUTHENTICATION MIDDLEWARE]
    .AddCookie(AuthScheme2);            // [MICROSOFT AUTHENTICATION MIDDLEWARE]
builder.Services
    .AddAuthorization(builder =>        // [MICROSOFT AUTHORIZATION MIDDLEWARE]
    {
        builder.AddPolicy("eu passport", pb =>          // ADD AUTHORIZATION POLICY
        {
            pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(AuthScheme)
            //.AddRequirements(new MyRequirement())       // ADD CUSTOM REQUIEREMENTS
            .RequireClaim("passport_type", "eur");
        });
    });

// *****************************************************************************
var app = builder.Build();
app.UseAuthentication();            // [MICROSOFT AUTHENTICATION MIDDLEWARE]
app.UseAuthorization();             // [MICROSOFT AUTHORIZATION MIDDLEWARE]

//app.Use((ctx, next) =>              // [+] ADDS MIDDLEWARE DELEGATE TO APP's REQUEST PIPELINE
//{
//    // ADDED MIDDLEWARE CHECKS
//    //if (ctx.Request.Path.StartsWithSegments("/login")
//    var anonymous = new[] { "/login", "/unsecure", "/signout" };
//    if (anonymous.Any(x => ctx.Request.Path.StartsWithSegments(x)))
//    {
//        return next();
//    }
//    if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthScheme))
//    {
//        // 401 Unauthorized = Unauthenticated
//        // - client must authenticate itself
//        ctx.Response.StatusCode = 401;
//        return Task.CompletedTask;
//    }
//    if (!ctx.User.HasClaim("passport_type", "eur"))
//    {
//        // 403 Forbidden
//        // - client does not have access rights to content
//        ctx.Response.StatusCode = 403;
//        return Task.CompletedTask;
//    }

//    return next();
//});

// [Minimal API] ***************************************************************
// [AllowAnonymous]
app.MapGet("/unsecure", (HttpContext ctx) =>
{
    // CHECK USER-CLAIM
    //return ctx.User.FindFirstValue("usr");
    //return ctx.User.FindFirst("usr")?.Value ?? "empty";
    return new
    {
        usr = ctx.User.FindFirst("usr")?.Value ?? null,
        passport_type = ctx.User.FindFirst("passport_type")?.Value ?? null,
    };
}).AllowAnonymous();

// [Authorize(Policy = "eu passport")]
app.MapGet("/sweden", (HttpContext ctx) =>
{
    // MOVED TO MIDDLEWARE CHECKS
    //if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthScheme))
    //{
    //    // 401 Unauthorized = Unauthenticated
    //    // - client must authenticate itself
    //    ctx.Response.StatusCode = 401;
    //    return "";
    //}

    //if (!ctx.User.HasClaim("passport_type", "eur"))
    //{
    //    // 403 Forbidden
    //    // - client does not have access rights to content
    //    ctx.Response.StatusCode = 403;
    //    return "";
    //}

    //ctx.Response.StatusCode = 200;
    return "allowed";
}).RequireAuthorization("eu passport");
app.MapGet("/norway", (HttpContext ctx) =>
{
    // MOVED TO MIDDLEWARE CHECKS
    //if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthScheme))
    //{
    //    // 401 Unauthorized = Unauthenticated
    //    // - client must authenticate itself
    //    ctx.Response.StatusCode = 401;
    //    return "";
    //}

    //if (!ctx.User.HasClaim("passport_type", "NOR"))
    //{
    //    // 403 Forbidden
    //    // - client does not have access rights to content
    //    ctx.Response.StatusCode = 403;
    //    return "";
    //}

    //ctx.Response.StatusCode = 200;
    return "allowed";
});
// [AuthScheme(AuthScheme2)]            : TODO (ADD SERVICE FOR NEW ATTRIBUTES)
// [AuthClaim("passport_type", "eur")]  : TODO (ADD SERVICE FOR NEW ATTRIBUTES)
app.MapGet("/denmark", (HttpContext ctx) =>
{
    // MOVED TO MIDDLEWARE CHECKS
    //if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthScheme2))
    //{
    //    // 401 Unauthorized = Unauthenticated
    //    // - client must authenticate itself
    //    ctx.Response.StatusCode = 401;
    //    return "";
    //}

    //if (!ctx.User.HasClaim("passport_type", "eur"))
    //{
    //    // 403 Forbidden
    //    // - client does not have access rights to content
    //    ctx.Response.StatusCode = 403;
    //    return "";
    //}

    //ctx.Response.StatusCode = 200;
    return "allowed";
});

// [AllowAnonymous]
app.MapGet("/login", async (HttpContext ctx) =>
{
    // CREATE USER-CLAIM
    var claims = new List<Claim>();
    claims.Add(new Claim("usr", "dotnet"));
    claims.Add(new Claim("passport_type", "eur"));
    var identity = new ClaimsIdentity(claims, AuthScheme);
    var user = new ClaimsPrincipal(identity);

    // [MICROSOFT AUTHENTICATION MIDDLEWARE]
    await ctx.SignInAsync(AuthScheme, user);
}).AllowAnonymous();
// [AllowAnonymous]
app.MapGet("/signout", async (HttpContext ctx) =>
{
    // [MICROSOFT AUTHENTICATION MIDDLEWARE]
    await ctx.SignOutAsync();
});

// *****************************************************************************
app.Run();

// *****************************************************************************
// ADD CUSTOM REQUIEREMENTS
public class MyRequirement : IAuthorizationRequirement {}
public class MyRequirementHandler : AuthorizationHandler<MyRequirement>
{
    // INJECT SERVICES
    // - communication with database, cache, ...
    public MyRequirementHandler() 
    {
        
    }
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MyRequirement requirement)
    {
        //context.User;
        context.Succeed(new MyRequirement());
        return Task.CompletedTask;
    }
}
