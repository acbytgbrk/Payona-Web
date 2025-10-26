#pragma warning disable CS8618, IDE0052

using Microsoft.EntityFrameworkCore;
using Payona.API.Models;

namespace Payona.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<DormInfo> DormInfos { get; set; }
    public DbSet<Fingerprint> Fingerprints { get; set; }
    public DbSet<MealRequest> MealRequests { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Surname).IsRequired().HasMaxLength(50);
        });
        
        // ✅ User - UserProfile (1-1)
        modelBuilder.Entity<DormInfo>(entity =>
        {
            entity.Property(p => p.Gender).IsRequired();
            entity.Property(p => p.City).IsRequired();
            entity.Property(p => p.Dorm).IsRequired();
        });

        // User - UserProfile (1-1)
        modelBuilder.Entity<User>()
            .HasOne(u => u.DormInfo)
            .WithOne(p => p.User)
            .HasForeignKey<DormInfo>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Fingerprint
        modelBuilder.Entity<Fingerprint>(entity =>
        {
            entity.ToTable("fingerprints");
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Fingerprints)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MealRequest
        modelBuilder.Entity<MealRequest>(entity =>
        {
            entity.ToTable("meal_requests");
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.MealRequests)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Match
        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("matches");
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Fingerprint)
                .WithMany()
                .HasForeignKey(e => e.FingerprintId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.MealRequest)
                .WithMany()
                .HasForeignKey(e => e.MealRequestId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Giver)
                .WithMany(u => u.GivenMatches)
                .HasForeignKey(e => e.GiverId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Receiver)
                .WithMany(u => u.ReceivedMatches)
                .HasForeignKey(e => e.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Message
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(e => e.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Match)
                .WithMany(m => m.Messages)
                .HasForeignKey(e => e.MatchId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}