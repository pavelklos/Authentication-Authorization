// *****************************************************************************
using IdentityManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
// *****************************************************************************
//Add services to the container.
//builder.Services.AddIdentity<IdentityUser, IdentityRole>()
//    .AddDefaultTokenProviders();
builder.Services.AddDataProtection();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddAuthorization(builder =>
{
    builder.AddPolicy("manager", pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
            .RequireClaim("role", "manager");
    });
});

builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// *****************************************************************************
var app = builder.Build();
// *****************************************************************************
// Configure the HTTP request pipeline.
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Minimal API
app.MapGet("/", () => "Hello World!");
app.MapGet("/protected", () => "Something protected!")
    .RequireAuthorization("manager");
//app.MapGet("/test", (
//    UserManager<IdentityUser> userMgr,
//    SignInManager<IdentityUser> signMgr
//    //DataProtectorTokenProvider<IdentityUser> tp
//    ) =>
//    {
//        // USER MANAGER from IDENTITY
//        //userMgr.PasswordHasher;
//        //userMgr.AddClaimAsync();
//        //userMgr.ChangeEmailAsync();
//        //userMgr.ChangePasswordAsync();
//        //userMgr.ResetPasswordAsync();
//        //userMgr.GenerateChangeEmailTokenAsync();
//        //userMgr.GeneratePasswordResetTokenAsync();
//        //userMgr.FindByEmailAsync();
//        //userMgr.FindByIdAsync();
//        //userMgr.FindByLoginAsync();
//        //userMgr.FindByNameAsync();
//        //userMgr.CreateAsync();

//        // SIGNIN MANAGER from IDENTITY
//        //signMgr.SignInAsync();
//        //signMgr.SignOutAsync();
//        //signMgr.PasswordSignInAsync();

//        // TOKEN PROVIDER
//        //tp.GenerateAsync();
//    });

app.MapGet("/register", async (
    string username,
    string password,
    IPasswordHasher<User> hasher,
    Database db,
    HttpContext ctx) =>
{
    // create user & hash password
    var user = new User() { Username = username };
    //user.Claims.Add(new UserClaim() { Type = "CLAIM-TYPE", Value = "CLAIM-VALUE"});
    user.PasswordHash = hasher.HashPassword(user, password);

    // save user to database-file
    await db.PutAsync(user);

    // sign-in
    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        UserHelper.Convert(user)
        );

    return user;
});

app.MapGet("/login", async (
    string username,
    string password,
    IPasswordHasher<User> hasher,
    Database db,
    HttpContext ctx) =>
{
    // get user
    var user = await db.GetUserAsync(username);

    // verify hashed password
    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
    if (result == PasswordVerificationResult.Failed)
    {
        return "bad credentials";
    };

    // sign-in
    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        UserHelper.Convert(user)
        );

    return "logged in";
});

app.MapGet("/promote", async (
    string username,
    Database db) =>
{
    // get user
    var user = await db.GetUserAsync(username);

    // add claims
    user.Claims.Add(new UserClaim() { Type = "role", Value = "manager" });

    // save user
    await db.PutAsync(user);

    return "promoted";
});

app.MapGet("/start-password-reset", async (
    string username,
    Database db,
    IDataProtectionProvider provider
    ) =>
{
    var protector = provider.CreateProtector("PasswordReset");
    var user = await db.GetUserAsync(username);
    return protector.Protect(user.Username);
});

app.MapGet("/end-password-reset", async (
    string username,
    string password,
    string hash,
    Database db,
    IPasswordHasher<User> hasher,
    IDataProtectionProvider provider
    ) =>
{
    var protector = provider.CreateProtector("PasswordReset");
    var hashUsername = protector.Unprotect(hash);
    if (hashUsername != username)
    {
        return "bad hash";
    }

    var user = await db.GetUserAsync(username);
    user.PasswordHash = hasher.HashPassword(user, password);
    await db.PutAsync(user);

    return "password reset";
});

// *****************************************************************************
app.Run();
// *****************************************************************************

public class UserHelper
{
    public static ClaimsPrincipal Convert(User user)
    {
        // create new claim 'username'
        var claims = new List<Claim>()
        {
            new Claim("username", user.Username),
        };

        // add existing claims from 'user'
        claims.AddRange(user.Claims.Select(x => new Claim(x.Type, x.Value)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }
}
