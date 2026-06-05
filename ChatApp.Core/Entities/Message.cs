using System;
using ChatApp.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Core.Entities
{
    /// <summary>
    /// Represents a chat message within the application.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Primary key for the message.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The textual content of the message.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the message was sent.
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Identifier of the sender (foreign key to ApplicationUser).
        /// </summary>
        public string SenderId { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to the sender user.
        /// </summary>
        public ApplicationUser? Sender { get; set; }
    }
}
