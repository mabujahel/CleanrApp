using CleanrApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanrApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Building> Buildings => Set<Building>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<CleaningRequest> CleaningRequests => Set<CleaningRequest>();
        public DbSet<RequestPhoto> RequestPhotos => Set<RequestPhoto>();
        public DbSet<RequestNote> RequestNotes => Set<RequestNote>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CleaningRequest>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.CustomerRequests)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CleaningRequest>()
                .HasOne(r => r.Supervisor)
                .WithMany(u => u.SupervisorRequests)
                .HasForeignKey(r => r.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CleaningRequest>()
                .HasOne(r => r.Cleaner)
                .WithMany(u => u.CleanerRequests)
                .HasForeignKey(r => r.CleanerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Unit>()
                .HasOne(u => u.Customer)
                .WithMany(u => u.Units)
                .HasForeignKey(u => u.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<RequestPhoto>()
                .HasOne(p => p.UploadedBy)
                .WithMany()
                .HasForeignKey(p => p.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RequestNote>()
                .HasOne(n => n.Author)
                .WithMany()
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
