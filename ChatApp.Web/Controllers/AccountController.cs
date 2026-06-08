using ChatApp.Core.Common;
using ChatApp.Web.Models;
using ChatApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _apiClient;

    public AccountController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("JwtToken") is not null)
        {
            return RedirectToAction("Index", "Chat");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var result = await _apiClient.PostAsync<object, ApiResponse<AuthResponse>>(
            "/api/auth/login",
            new { model.UsernameOrEmail, model.Password });

        var response = result.Data;
        if (!result.IsSuccess || response?.Success != true || response.Data is null)
        {
            model.ErrorMessage = result.ErrorMessage
                ?? response?.Message
                ?? FormatErrors(response?.Errors)
                ?? "Invalid credentials.";
            return View(model);
        }

        HttpContext.Session.SetString("JwtToken", response.Data.Token);
        HttpContext.Session.SetString("UserId", response.Data.UserId);
        HttpContext.Session.SetString("Username", response.Data.Username);
        HttpContext.Session.SetString("Email", response.Data.Email);

        return RedirectToAction("Index", "Chat");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Password != model.ConfirmPassword)
        {
            model.ErrorMessage = "Passwords do not match.";
            return View(model);
        }

        var result = await _apiClient.PostAsync<object, ApiResponse<AuthResponse>>(
            "/api/auth/register",
            new { model.Username, model.Email, model.Password });

        var response = result.Data;
        if (!result.IsSuccess || response?.Success != true || response.Data is null)
        {
            model.ErrorMessage = result.ErrorMessage
                ?? response?.Message
                ?? FormatErrors(response?.Errors)
                ?? "Registration failed.";
            return View(model);
        }

        HttpContext.Session.SetString("JwtToken", response.Data.Token);
        HttpContext.Session.SetString("UserId", response.Data.UserId);
        HttpContext.Session.SetString("Username", response.Data.Username);
        HttpContext.Session.SetString("Email", response.Data.Email);

        return RedirectToAction("Index", "Chat");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private static string? FormatErrors(IEnumerable<string>? errors)
    {
        var list = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
        return list is { Count: > 0 } ? string.Join(" ", list) : null;
    }
}
