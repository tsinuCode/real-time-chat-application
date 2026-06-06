using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Controllers;

public class ProfileController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["Username"] = HttpContext.Session.GetString("Username") ?? "User";
        ViewData["Email"] = HttpContext.Session.GetString("Email") ?? "";
        ViewData["UserId"] = HttpContext.Session.GetString("UserId");

        return PartialView("_ProfileDrawer");
    }
}
