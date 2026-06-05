using ChatApp.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Data;

public class ChatAppDbContext : IdentityDbContext<ApplicationUser>
{
    public ChatAppDbContext(DbContextOptions<ChatAppDbContext> options) : base(options)
    {
    }

    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ChatGroup> ChatGroups => Set<ChatGroup>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Content).HasMaxLength(2000).IsRequired();
            entity.HasIndex(m => new { m.SenderId, m.ReceiverId });
            entity.HasIndex(m => m.GroupId);

            entity.HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Group)
                .WithMany(g => g.Messages)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ChatGroup>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.GroupName).HasMaxLength(100).IsRequired();
            entity.HasIndex(g => g.GroupName);

            entity.HasOne(g => g.Creator)
                .WithMany(u => u.CreatedGroups)
                .HasForeignKey(g => g.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(gm => gm.Id);
            entity.HasIndex(gm => new { gm.GroupId, gm.UserId }).IsUnique();

            entity.HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
