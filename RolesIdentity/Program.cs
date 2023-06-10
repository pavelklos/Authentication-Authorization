using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// *****************************************************************************
var builder = WebApplication.CreateBuilder(args);
// *****************************************************************************
// Add services to the container.

//builder.Services.AddAuthentication()
//    .AddCookie("cookie");
//builder.Services.AddAuthorization();
// ADD IDENTITY FRAMEWORK
// - UseInMemoryDatabase
// - configure Identity System
builder.Services.AddDbContext<IdentityDbContext>(c => c.UseInMemoryDatabase("my_db"));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
{
    //o.ClaimsIdentity.
    o.User.RequireUniqueEmail = false;

    o.Password.RequireDigit = false;             // 123
    o.Password.RequiredLength = 4;               // 4 = LENGTH
    o.Password.RequireLowercase = false;         // abc
    o.Password.RequireUppercase = false;         // ABC
    o.Password.RequireNonAlphanumeric = false;   // !@#
})
.AddEntityFrameworkStores<IdentityDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllers();

// *****************************************************************************
var app = builder.Build();
// *****************************************************************************
// Configure the HTTP request pipeline.
// ADD IDENTIFY USER & ROLE
using (var scope = app.Services.CreateScope())
{
    // ROLE
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await roleMgr.CreateAsync(new IdentityRole() { Name = "admin" });
    // USER
    var usrMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var user = new IdentityUser() { UserName = "test@test.com", Email = "test@test.com" };
    await usrMgr.CreateAsync(user, password: "password");
    await usrMgr.AddToRoleAsync(user, "admin");
    //await usrMgr.AddClaimAsync();
}

app.UseHttpsRedirection();
//app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// *****************************************************************************
app.Run();
// *****************************************************************************
