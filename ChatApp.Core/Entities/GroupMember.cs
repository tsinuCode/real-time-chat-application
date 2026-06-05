using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Core.Entities
{
    /// <summary>
    /// Represents a membership of a user in a chat group.
    /// </summary>
    public class GroupMember
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Foreign key to the associated ChatGroup.
        /// </summary>
        [Required]
        public Guid ChatGroupId { get; set; }

        /// <summary>
        /// Navigation property to the ChatGroup.
        /// </summary>
        [ForeignKey(nameof(ChatGroupId))]
        public ChatGroup ChatGroup { get; set; }

        /// <summary>
        /// Foreign key to the member ApplicationUser.
        /// </summary>
        [Required]
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Navigation property to the ApplicationUser.
        /// </summary>
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// Role of the user within the group (e.g., Owner, Admin, Member).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Member";

        /// <summary>
        /// Date and time when the user joined the group.
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
