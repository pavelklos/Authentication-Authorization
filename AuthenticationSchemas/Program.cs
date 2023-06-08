using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// *****************************************************************************
var builder = WebApplication.CreateBuilder(args);
// *****************************************************************************
// Add services to the container.
builder.Services.AddAuthentication()    // [+] Authentication services
    .AddCookie("local");                //     authentication scheme = 'local'
builder.Services.AddAuthorization();    // [+] Authorization Policy services

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
app.MapGet("/", (ctx) => Task.FromResult("Hello World!"));

app.MapGet("/login-local", async (ctx) =>
{
    var claims = new List<Claim>();
    claims.Add(new Claim("usr", "anton"));
    var identity = new ClaimsIdentity(claims, "cookie");
    var user = new ClaimsPrincipal(identity);

    // Sign in principal for specified scheme
    await ctx.SignInAsync("local", user); // authentication scheme = 'local' 
});

// *****************************************************************************
app.Run();
// *****************************************************************************
