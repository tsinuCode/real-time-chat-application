using Microsoft.AspNetCore.Identity;

namespace ChatApp.Core.Entities
{
    /// <summary>
    /// Represents an application user extending ASP.NET Core IdentityUser.
    /// Add custom profile properties here as needed.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Example of a custom property:
        // public string DisplayName { get; set; } = string.Empty;
    }
}
