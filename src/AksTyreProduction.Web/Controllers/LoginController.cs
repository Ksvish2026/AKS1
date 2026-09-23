using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AksTyreProduction.Web.Controllers;

public class LoginController : Controller
{
    [AllowAnonymous, HttpGet("/Login")]
    public IActionResult Index(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Dashboard");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [AllowAnonymous, HttpPost("/Login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string username, string password, string? returnUrl = null)
    {
        if (username != "Admin" || password != "Admin")
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Error = "Incorrect username or password.";
            return View();
        }
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "Admin"), new Claim(ClaimTypes.Role, "Administrator")], CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        Response.Cookies.Append("DemoRole", "Administrator", new CookieOptions { IsEssential = true, SameSite = SameSiteMode.Strict });
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("DemoRole");
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous, HttpGet] public IActionResult Denied() => View("Index");
}
