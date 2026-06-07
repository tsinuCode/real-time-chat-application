using ChatApp.Core.Common;
using ChatApp.Core.DTOs.Groups;
using ChatApp.Core.DTOs.Users;
using ChatApp.Web.Models;
using ChatApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Controllers;

public class GroupsController : Controller
{
    private readonly ApiClient _apiClient;

    public GroupsController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private bool TryGetSession(out string username, out string userId)
    {
        if (HttpContext.Session.GetString("JwtToken") is null)
        {
            username = userId = string.Empty;
            return false;
        }

        username = HttpContext.Session.GetString("Username") ?? "User";
        userId = HttpContext.Session.GetString("UserId") ?? string.Empty;
        return true;
    }

    private void SetGroupsViewData(string username)
    {
        ViewData["Username"] = username;
        ViewData["ActiveNav"] = "Groups";
        ViewData["BodyClass"] = "directory-page-body";
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!TryGetSession(out var username, out _))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["Title"] = "Groups";
        SetGroupsViewData(username);

        var model = new GroupsIndexViewModel();
        var result = await _apiClient.GetAsync<ApiResponse<IReadOnlyList<GroupDto>>>("/api/groups");

        if (!result.IsSuccess || result.Data?.Success != true || result.Data.Data is null)
        {
            model.ErrorMessage = result.ErrorMessage ?? result.Data?.Message ?? "Failed to load groups.";
            return View(model);
        }

        model.Groups = result.Data.Data;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, string? query)
    {
        if (!TryGetSession(out var username, out var currentUserId))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["Title"] = "Group Details";
        SetGroupsViewData(username);

        var model = new GroupDetailsViewModel { SearchQuery = query };

        var groupResult = await _apiClient.GetAsync<ApiResponse<GroupDetailDto>>($"/api/groups/{id}");
        if (!groupResult.IsSuccess || groupResult.Data?.Success != true || groupResult.Data.Data is null)
        {
            TempData["ErrorMessage"] = groupResult.ErrorMessage ?? groupResult.Data?.Message ?? "Group not found.";
            return RedirectToAction(nameof(Index));
        }

        model.Group = groupResult.Data.Data;

        var directoryPath = string.IsNullOrWhiteSpace(query)
            ? "/api/directory/users"
            : $"/api/directory/search?query={Uri.EscapeDataString(query)}";

        var usersResult = await _apiClient.GetAsync<ApiResponse<IReadOnlyList<DirectoryUserSummaryDto>>>(directoryPath);
        if (usersResult.IsSuccess && usersResult.Data?.Success == true && usersResult.Data.Data is not null)
        {
            var memberIds = model.Group.Members.Select(m => m.UserId).ToHashSet();
            model.AvailableUsers = usersResult.Data.Data
                .Where(u => !memberIds.Contains(u.Id) && u.Id != currentUserId)
                .ToList();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(int groupId, string userId, string? query)
    {
        if (!TryGetSession(out _, out _))
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _apiClient.PostAsync<AddGroupMemberDto, ApiResponse<object>>(
            $"/api/groups/{groupId}/members",
            new AddGroupMemberDto { UserId = userId });

        if (!result.IsSuccess || result.Data?.Success != true)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? result.Data?.Message ?? "Failed to add member.";
        }
        else
        {
            TempData["SuccessMessage"] = "Member added successfully.";
        }

        return RedirectToAction(nameof(Details), new { id = groupId, query });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "NewGroup")] CreateGroupDto newGroup)
    {
        if (!TryGetSession(out var username, out _))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["Title"] = "Groups";
        SetGroupsViewData(username);

        var model = new GroupsIndexViewModel { NewGroup = newGroup };

        if (!ModelState.IsValid)
        {
            model.ErrorMessage = "Group name must be between 3 and 100 characters.";
            return await ReloadGroupsView(model);
        }

        var result = await _apiClient.PostAsync<CreateGroupDto, ApiResponse<GroupDto>>(
            "/api/groups", newGroup);

        if (!result.IsSuccess || result.Data?.Success != true)
        {
            model.ErrorMessage = result.ErrorMessage ?? result.Data?.Message ?? "Failed to create group.";
            return await ReloadGroupsView(model);
        }

        TempData["SuccessMessage"] = "Group created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(int groupId)
    {
        if (!TryGetSession(out _, out _))
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _apiClient.PostAsync<object, ApiResponse<object>>(
            $"/api/groups/{groupId}/leave", new { });

        if (!result.IsSuccess || result.Data?.Success != true)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? result.Data?.Message ?? "Failed to leave group.";
        }
        else
        {
            TempData["SuccessMessage"] = "Left group successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReloadGroupsView(GroupsIndexViewModel model)
    {
        var result = await _apiClient.GetAsync<ApiResponse<IReadOnlyList<GroupDto>>>("/api/groups");
        if (result.IsSuccess && result.Data?.Success == true && result.Data.Data is not null)
        {
            model.Groups = result.Data.Data;
        }

        return View("Index", model);
    }
}
