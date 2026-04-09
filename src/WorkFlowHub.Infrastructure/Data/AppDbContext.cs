using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Core.Models;

namespace WorkFlowHub.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<FileRecord> FileRecords => Set<FileRecord>();
    public DbSet<TaskComment> Comments => Set<TaskComment>();
    public DbSet<TaskLabel> Labels => Set<TaskLabel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.Description).HasMaxLength(2000);
            e.HasOne(p => p.Owner)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(p => !p.IsDeleted);
        });

        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Title).HasMaxLength(300).IsRequired();
            e.Property(t => t.Description).HasMaxLength(2000);
            e.Property(t => t.Status).HasConversion<string>();
            e.Property(t => t.Priority).HasConversion<string>();
            e.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
            e.HasQueryFilter(t => !t.IsDeleted);
        });

        modelBuilder.Entity<TaskComment>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Body).HasMaxLength(2000).IsRequired();
            e.HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TaskLabel>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Name).HasMaxLength(50).IsRequired();
            e.Property(l => l.Color).HasMaxLength(20).IsRequired();
            e.HasOne(l => l.Task)
                .WithMany(t => t.Labels)
                .HasForeignKey(l => l.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FileRecord>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.FileName).HasMaxLength(500).IsRequired();
            e.Property(f => f.BlobUrl).HasMaxLength(2000).IsRequired();
            e.HasOne(f => f.Uploader)
                .WithMany(u => u.FileRecords)
                .HasForeignKey(f => f.UploadedBy)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
