// *****************************************************************************
using IdentityManagement;

var builder = WebApplication.CreateBuilder(args);
// *****************************************************************************
// Add services to the container.
builder.Services.AddSingleton<Database>();


// *****************************************************************************
var app = builder.Build();
// *****************************************************************************
// Configure the HTTP request pipeline.
//app.UseHttpsRedirection();

// Minimal API
app.MapGet("/", () =>
{
    return "Hello World!";
});

// *****************************************************************************
app.Run();
// *****************************************************************************
