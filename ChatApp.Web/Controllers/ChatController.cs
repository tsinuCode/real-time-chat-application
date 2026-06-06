using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Controllers;

public class ChatController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["Title"] = "Chats";
        ViewData["Username"] = HttpContext.Session.GetString("Username") ?? "User";
        ViewData["UserId"] = HttpContext.Session.GetString("UserId");
        ViewData["JwtToken"] = token;

        return View();
    }
}
