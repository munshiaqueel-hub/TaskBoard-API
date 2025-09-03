using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Models;
public class TaskBoardDbContext : DbContext
{
    public TaskBoardDbContext(DbContextOptions<TaskBoardDbContext> options) : base(options) { }
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Column> Columns { get; set; }
    public DbSet<BoardTask> Tasks { get; set; }
    public DbSet<TaskImage> TaskImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskImage>(entity =>
        {
            entity.HasKey(ti => ti.Id);

            entity.HasOne(ti => ti.Task)
                  .WithMany(t => t.Images)
                  .HasForeignKey(ti => ti.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => new { rt.UserId, rt.TokenHash })
            .IsUnique();

        // Column -> Tasks (1:N)
        modelBuilder.Entity<Column>()
            .HasMany(c => c.Tasks)
            .WithOne(t => t.Column)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        // Task -> Images (1:N)
        modelBuilder.Entity<BoardTask>()
            .HasMany(t => t.Images)
            .WithOne(i => i.Task)
            .HasForeignKey(i => i.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<Column>()
            .HasIndex(c => c.Name)
            .IsUnique();
    }

}

