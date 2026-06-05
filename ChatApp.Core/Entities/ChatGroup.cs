using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Core.Entities
{
    public class ChatGroup
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
