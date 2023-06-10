using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

// *****************************************************************************
var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddDataProtection();
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<AuthServiceCustom>();
builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie");
//.AddCookie("cookie");
//// Add services to the container.
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// *****************************************************************************
var app = builder.Build();
app.UseAuthentication();
//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
//app.UseHttpsRedirection();

// [CUSTOM AUTH-SERVICE]
// Adds  middleware delegate defined in-line to the application's request pipeline
// - LOAD COOKIE & DECRYPT PAYLOAD
//app.Use((ctx, next) =>
//{
//    var idp = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
//    var protector = idp.CreateProtector("auth-cookie");

//    var authCookie = ctx.Request.Headers["cookie"].FirstOrDefault(x => x.StartsWith("auth"));
//    // CHECK IF THERE IS COOKIE
//    if (authCookie != null)
//    {
//        var protectedPayload = authCookie.Split("=").Last();
//        var payload = protector.Unprotect(protectedPayload);
//        var parts = payload.Split(':');
//        var key = parts[0];
//        var value = parts[1];

//        // CREATE USER
//        var claims = new List<Claim>();
//        claims.Add(new Claim("authCookie", authCookie));
//        claims.Add(new Claim("protectedPayload", protectedPayload));
//        claims.Add(new Claim("payload", payload));
//        claims.Add(new Claim(key, value));
//        var identity = new ClaimsIdentity(claims);
//        ctx.User = new ClaimsPrincipal(identity);
//    }

//    return next();
//});


// [Minimal API] ***************************************************************
//app.MapGet("/username", (HttpContext ctx, IDataProtectionProvider idp) =>
app.MapGet("/username", (HttpContext ctx) =>
{
    // GET USER
    return ctx.User.FindFirstValue("usr");

    // [CUSTOM AUTH-SERVICE]
    //return ctx.User;
    //return ctx.User.Identity;
    //return ctx.User.FindFirst("usr").Value;
    //return ctx.User.FindFirstValue("protectedPayload");
    //return ctx.User.FindFirstValue("payload");
    //return ctx.User.FindFirstValue("usr");
    //return new
    //{
    //    usr = ctx.User.FindFirstValue("usr"),
    //    payload = ctx.User.FindFirstValue("payload"),
    //    protectedPayload = ctx.User.FindFirstValue("protectedPayload"),
    //    authCookie = ctx.User.FindFirstValue("authCookie"),
    //};

    // [DataProtection]
    //var protector = idp.CreateProtector("auth-cookie");
    //var authCookie = ctx.Request.Headers["cookie"].FirstOrDefault(x => x.StartsWith("auth"));
    //var protectedPayload = authCookie.Split("=").Last();
    //var payload = protector.Unprotect(protectedPayload);
    //var parts = payload.Split(':');
    //var key = parts[0];
    //var value = parts[1];
    //return new
    //{
    //    authCookie,
    //    payload,
    //    key,
    //    value,
    //    headers = ctx.Request.Headers.Count
    //};

    // [PLAIN TEXT] "auth=usr:anton"
    //var authCookie = ctx.Request.Headers["cookie"].FirstOrDefault(x => x.StartsWith("auth"));
    //var payload = authCookie.Split("=").Last();
    //var parts = payload.Split(':');
    //var key = parts[0];
    //var value = parts[1];
    //return new
    //{
    //    authCookie,
    //    payload,
    //    key,
    //    value,
    //    headers = ctx.Request.Headers.Count
    //};

    //return "anton";
});

//app.MapGet("/login", (HttpContext ctx, IDataProtectionProvider idp) =>
//app.MapGet("/login", (AuthServiceCustom auth) =>
app.MapGet("/login", async (HttpContext ctx) =>
{
    // CREATE USER
    var claims = new List<Claim>();
    claims.Add(new Claim("usr", "dotnet"));
    var identity = new ClaimsIdentity(claims, "cookie");
    var user = new ClaimsPrincipal(identity);

    // [MICROSOFT AUTHENTICATION MIDDLEWARE]
    await ctx.SignInAsync("cookie", user);

    // [CUSTOM AUTH-SERVICE]
    //auth.SignIn();

    // [DataProtection]
    //var protector = idp.CreateProtector("auth-cookie");
    //ctx.Response.Headers["set-cookie"] = $"auth={protector.Protect("usr:anton")}";

    // [PLAIN TEXT] "auth=usr:anton"
    //ctx.Response.Headers["set-cookie"] = "auth=usr:anton";

    return "ok";
});

app.MapGet("/signout", async (HttpContext ctx) =>
{
    // [MICROSOFT AUTHENTICATION MIDDLEWARE]
    await ctx.SignOutAsync();

    return "ok";
});



// *****************************************************************************
app.Run();

// *****************************************************************************

// CUSTOM AUTH-SERVICE
public class AuthServiceCustom
{
    private readonly IDataProtectionProvider _idp;
    private readonly IHttpContextAccessor _accessor;

    public AuthServiceCustom(IDataProtectionProvider idp, IHttpContextAccessor accessor)
    {
        this._idp = idp;
        this._accessor = accessor;
    }

    public void SignIn()
    {
        var protector = _idp.CreateProtector("auth-cookie");
        _accessor.HttpContext.Response.Headers["set-cookie"] = $"auth={protector.Protect("usr:anton")}";
    }
}