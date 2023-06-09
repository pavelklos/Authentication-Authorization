using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

// *****************************************************************************
var builder = WebApplication.CreateBuilder(args);
// *****************************************************************************
// Add services to the container.
builder.Services.AddAuthentication()        // [+] Authentication services
                                            //.AddCookie("visitor")                 //     authentication scheme = 'visitor'
    .AddScheme<CookieAuthenticationOptions, VisitorAuthHandler>("visitor", o => { })
    .AddCookie("local")                     // authentication scheme = 'local'
    .AddCookie("patreon-cookie")            // authentication scheme = 'patreon-cookie'
    .AddOAuth("external-patreon", o =>      // Adds OAuth 2.0 based authentication using specified authentication scheme
    {
        o.SignInScheme = "patreon-cookie";


        o.ClientId = "id";
        o.ClientSecret = "secret";

        // Mock OAuth2 / OpenID Connect Login
        // - https://oauth.mocklab.io/
        o.AuthorizationEndpoint = "https://oauth.mocklab.io/oauth/authorize";
        o.TokenEndpoint = "https://oauth.mocklab.io/oauth/token";
        o.UserInformationEndpoint = "https://oauth.mocklab.io/userinfo";

        o.CallbackPath = "/cb-patreon";

        o.Scope.Add("profile");
        o.SaveTokens = true;
    });
builder.Services.AddAuthorization(b =>      // [+] Authorization Policy services
{
    // customer = local, visitor
    b.AddPolicy("customer", p =>
    {
        p.AddAuthenticationSchemes("patreon-cookie", "local", "visitor")
            .RequireAuthenticatedUser();
    });
    // user = local
    b.AddPolicy("user", p =>
    {
        p.AddAuthenticationSchemes("local")
            .RequireAuthenticatedUser();
    });
});

// *****************************************************************************
var app = builder.Build();
// *****************************************************************************
// Configure the HTTP request pipeline.
//app.UseHttpsRedirection();
app.UseAuthentication();    // [+] Authentication to HTTP application pipeline
app.UseAuthorization();     // [+] Authorization to HTTP application pipeline

// *****************************************************************************
// Minimal API
// *****************************************************************************
app.MapGet("/", (ctx) => Task.FromResult("Hello World!"))
    .RequireAuthorization("customer");

app.MapGet("/login-local", async (ctx) =>
{
    // 'LOCAL' SCHEME
    var claims = new List<Claim>();
    claims.Add(new Claim("usr", "anton"));
    var identity = new ClaimsIdentity(claims, "local");
    var user = new ClaimsPrincipal(identity);

    // Sign in principal for specified scheme
    await ctx.SignInAsync("local", user); // authentication scheme = 'local' 
});

app.MapGet("/login-patreon", async (ctx) =>
{
    // SIGN-IN EXTERNALLY BY 'OAuth2'
    await ctx.ChallengeAsync("external-patreon", new AuthenticationProperties()
    {
        RedirectUri = "/"
    });
}).RequireAuthorization("user");

// *****************************************************************************
app.Run();
// *****************************************************************************

public class VisitorAuthHandler : CookieAuthenticationHandler
{
    public VisitorAuthHandler(
        IOptionsMonitor<CookieAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {

    }
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var result = await base.HandleAuthenticateAsync(); // AuthenticateResult
        if (result.Succeeded)
        {
            return result;
        }

        // 'VISITOR' SCHEME
        var claims = new List<Claim>();
        claims.Add(new Claim("usr", "anton"));
        var identity = new ClaimsIdentity(claims, "visitor");
        var user = new ClaimsPrincipal(identity);

        await Context.SignInAsync("visitor", user);

        return AuthenticateResult.Success(new AuthenticationTicket(user, "visitor"));
    }
}