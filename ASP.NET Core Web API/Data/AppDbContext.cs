using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public AppDbContext() { }

        // DbSet properties for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<StudyGroup> StudyGroups { get; set; }
        public DbSet<JoinRequest> JoinRequests { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasKey(u => u.ID);
            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .HasMaxLength(100);
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasMaxLength(50);

            // Configure StudyGroup entity
            modelBuilder.Entity<StudyGroup>()
                .HasKey(sg => sg.ID);
            modelBuilder.Entity<StudyGroup>()
                .Property(sg => sg.Subject)
                .HasMaxLength(200);
            modelBuilder.Entity<StudyGroup>()
                .Property(sg => sg.Location)
                .HasMaxLength(200);
            modelBuilder.Entity<StudyGroup>()
                .Property(sg => sg.Description)
                .HasMaxLength(500);
            modelBuilder.Entity<StudyGroup>()
                .Property(sg => sg.MeetingType)
                .HasMaxLength(50);

            // Configure StudyGroup - User relationship
            modelBuilder.Entity<StudyGroup>()
                .HasOne(sg => sg.User)
                .WithMany(u => u.StudyGroups)
                .HasForeignKey(sg => sg.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure JoinRequest entity
            modelBuilder.Entity<JoinRequest>()
                .HasKey(jr => jr.ID);
            modelBuilder.Entity<JoinRequest>()
                .Property(jr => jr.Status)
                .HasMaxLength(50);

            // Configure JoinRequest - User relationship
            modelBuilder.Entity<JoinRequest>()
                .HasOne(jr => jr.User)
                .WithMany(u => u.JoinRequests)
                .HasForeignKey(jr => jr.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure JoinRequest - StudyGroup relationship
            modelBuilder.Entity<JoinRequest>()
                .HasOne(jr => jr.StudyGroup)
                .WithMany(sg => sg.JoinRequests)
                .HasForeignKey(jr => jr.StudyGroupID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure GroupMember entity
            modelBuilder.Entity<GroupMember>()
                .HasKey(gm => gm.ID);

            // Configure GroupMember - User relationship
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMembers)
                .HasForeignKey(gm => gm.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure GroupMember - StudyGroup relationship
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.StudyGroup)
                .WithMany(sg => sg.GroupMembers)
                .HasForeignKey(gm => gm.StudyGroupID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Material entity
            modelBuilder.Entity<Material>()
                .HasKey(m => m.ID);
            modelBuilder.Entity<Material>()
                .Property(m => m.FileName)
                .HasMaxLength(255);
            modelBuilder.Entity<Material>()
                .Property(m => m.FileUrl)
                .HasMaxLength(500);

            // Configure Material - User relationship
            modelBuilder.Entity<Material>()
                .HasOne(m => m.User)
                .WithMany(u => u.Materials)
                .HasForeignKey(m => m.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Material - StudyGroup relationship
            modelBuilder.Entity<Material>()
                .HasOne(m => m.StudyGroup)
                .WithMany(sg => sg.Materials)
                .HasForeignKey(m => m.StudyGroupID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Comment entity
            modelBuilder.Entity<Comment>()
                .HasKey(c => c.ID);
            modelBuilder.Entity<Comment>()
                .Property(c => c.Content)
                .HasMaxLength(1000);

            // Configure Comment - User relationship
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Comment - StudyGroup relationship
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.StudyGroup)
                .WithMany(sg => sg.Comments)
                .HasForeignKey(c => c.StudyGroupID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
