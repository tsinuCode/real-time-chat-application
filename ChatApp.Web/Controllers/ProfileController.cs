using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Controllers;

public class ProfileController : Controller
{
    private bool TryGetSession(out string username, out string email, out string userId)
    {
        if (HttpContext.Session.GetString("JwtToken") is null)
        {
            username = email = userId = string.Empty;
            return false;
        }

        username = HttpContext.Session.GetString("Username") ?? "User";
        email = HttpContext.Session.GetString("Email") ?? "";
        userId = HttpContext.Session.GetString("UserId") ?? "";
        return true;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (!TryGetSession(out var username, out var email, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["Title"] = "Profile";
        ViewData["Username"] = username;
        ViewData["Email"] = email;
        ViewData["UserId"] = userId;
        ViewData["BodyClass"] = "profile-page-body";

        return View();
    }

    [HttpGet]
    public IActionResult Settings()
    {
        if (!TryGetSession(out var username, out var email, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["Title"] = "Settings";
        ViewData["Username"] = username;
        ViewData["Email"] = email;
        ViewData["UserId"] = userId;
        ViewData["BodyClass"] = "profile-page-body";

        return View();
    }
}
