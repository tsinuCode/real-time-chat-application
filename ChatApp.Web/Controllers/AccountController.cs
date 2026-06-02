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

        if (!result.IsSuccess)
        {
            model.ErrorMessage = result.ErrorMessage ?? "Login failed.";
            return View(model);
        }

        var response = result.Data;
        if (response?.Success != true || response.Data is null)
        {
            model.ErrorMessage = response?.Message ?? "Invalid credentials.";
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

        if (!result.IsSuccess)
        {
            model.ErrorMessage = result.ErrorMessage ?? "Registration failed.";
            return View(model);
        }

        var response = result.Data;
        if (response?.Success != true || response.Data is null)
        {
            model.ErrorMessage = response?.Message ?? "Registration failed.";
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
}
