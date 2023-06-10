using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace Roles.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    //public class HomeController : Controller
    {
        [HttpGet("/")]
        public string Index() => "Index Route";

        [HttpGet("/secret")]
        [Authorize(Roles = "admin")]
        public string Secret() => "Secret Route";

        [HttpGet("/user")]
        public object UserInfo()
        {
            var identity = HttpContext.User.Identity;
            var claims = HttpContext.User.Claims;
            var claim_name_identifier = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claim_first_my_role_claim = HttpContext.User.FindFirstValue("my-role-claim");
            var claims_my_role_claim = HttpContext.User.FindAll("my-role-claim");
            var isInRole_admin = HttpContext.User.IsInRole("admin");
            var isInRole_manager = HttpContext.User.IsInRole("manager");
           
            //return claims.Count().ToString();
            return new
            {
                //nameIdentifier = ClaimTypes.NameIdentifier,
                name = identity?.Name,
                claims_count = claims.Count(),
                claim_name_identifier,
                claim_first_my_role_claim,
                //claims = claims.Select(c => new { c.Type, c.Value }),
                claims_my_role_claim = claims_my_role_claim.Select(c => new { c.Type, c.Value }),
                isInRole_admin,
                isInRole_manager,
            };
        }
    }
}
