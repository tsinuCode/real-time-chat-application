using ChatApp.Core.Common;
using ChatApp.Core.DTOs.Users;
using ChatApp.Web.Models;
using ChatApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Controllers;

public class DirectoryController : Controller
{
    private readonly ApiClient _apiClient;

    public DirectoryController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

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
    public async Task<IActionResult> Index(string? query)
    {
        if (!TryGetSession(out var username, out _, out _))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["Title"] = "Directory";
        ViewData["Username"] = username;
        ViewData["ActiveNav"] = "Directory";
        ViewData["BodyClass"] = "directory-page-body";

        var path = string.IsNullOrWhiteSpace(query)
            ? "/api/directory/users"
            : $"/api/directory/search?query={Uri.EscapeDataString(query)}";

        var result = await _apiClient.GetAsync<ApiResponse<IReadOnlyList<DirectoryUserSummaryDto>>>(path);

        var model = new DirectoryIndexViewModel { Query = query };

        if (!result.IsSuccess || result.Data?.Success != true || result.Data.Data is null)
        {
            model.ErrorMessage = result.ErrorMessage ?? result.Data?.Message ?? "Failed to load users.";
            return View(model);
        }

        model.Users = result.Data.Data;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (!TryGetSession(out var username, out _, out _))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["Title"] = "User Profile";
        ViewData["Username"] = username;
        ViewData["ActiveNav"] = "Directory";
        ViewData["BodyClass"] = "directory-page-body";

        var result = await _apiClient.GetAsync<ApiResponse<DirectoryUserDetailDto>>(
            $"/api/directory/user/{Uri.EscapeDataString(id)}");

        if (!result.IsSuccess || result.Data?.Success != true || result.Data.Data is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new DirectoryDetailsViewModel { User = result.Data.Data });
    }
}
