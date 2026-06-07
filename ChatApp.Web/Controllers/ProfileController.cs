using ChatApp.Web.Models;
using ChatApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Controllers;

public class ProfileController : Controller
{
    private readonly ApiClient _apiClient;

    public ProfileController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("JwtToken") is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var meResult = await _apiClient.GetAsync<ApiResponse<UserViewModel>>("/api/users/me");
        if (!meResult.IsSuccess || meResult.Data?.Data is null)
        {
            TempData["Error"] = meResult.ErrorMessage ?? "Unable to load profile.";
            return RedirectToAction("Index", "Chat");
        }

        return View(meResult.Data.Data);
    }

    [HttpGet]
    public IActionResult Settings()
    {
        if (HttpContext.Session.GetString("JwtToken") is null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }
}
