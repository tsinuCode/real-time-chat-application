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

        var apiBaseUrl = Environment.GetEnvironmentVariable("CHAT_API_URL") ?? "https://localhost:7244";

        ViewData["Title"] = "Chats";
        ViewData["Username"] = HttpContext.Session.GetString("Username") ?? "User";
        ViewData["UserId"] = HttpContext.Session.GetString("UserId");
        ViewData["JwtToken"] = token;
        ViewData["ApiBaseUrl"] = apiBaseUrl;
        ViewData["HubUrl"] = $"{apiBaseUrl.TrimEnd('/')}/hubs/chat";

        return View();
    }
}
