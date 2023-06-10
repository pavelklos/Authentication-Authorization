using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Roles.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    //public class AccountController : Controller
    {
        [HttpGet("/login")]
        public IActionResult Login() =>

            // Manually create Claims Principle
            SignIn(new ClaimsPrincipal(                     // claims principal
                new ClaimsIdentity(                         // claims identity
                    new Claim[]                             // claims
                    {
                        // claim type-value
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                        new Claim("my-role-claim", "admin"),
                        new Claim("my-role-claim", "manager")
                    },
                    "cookie",                               // authentication type
                    nameType: null,                         // name type
                    roleType: "my-role-claim"               // role type
                )), authenticationScheme: "cookie");        // authentication scheme
    }
}
