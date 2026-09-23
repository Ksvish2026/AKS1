using Microsoft.AspNetCore.Mvc;
namespace AksTyreProduction.Web.Controllers;
public class RoleController:Controller
{
    [HttpPost,ValidateAntiForgeryToken]public IActionResult Switch(string role,string? returnUrl){var allowed=new[]{"Administrator","Management","Receiving","Inspector","Production Operator","QC","Dispatch"};if(allowed.Contains(role))Response.Cookies.Append("DemoRole",role,new CookieOptions{IsEssential=true,SameSite=SameSiteMode.Strict});return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl)?"/":returnUrl);}
}
