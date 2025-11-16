
using Microsoft.EntityFrameworkCore;
using TodoWebApp.Server.Models;


namespace TodoWebApp.Server.Data 
{
    public class TodoDbContext : DbContext 
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) 
            : base(options) { }

        public DbSet<Todo> Todos { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            base.OnModelCreating(modelBuilder);


            // Configure Todo Entity
            modelBuilder.Entity<Todo>(entity => {

                entity.HasKey(t => t.Id);

                entity.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.Description)
                    .HasMaxLength(2000);

                entity.Property(t => t.Category)
                    .HasMaxLength(100);

                entity.Property(t => t.Tags)
                    .HasMaxLength(500);

                entity.Property(t => t.IsComplete)
                    .HasDefaultValue(false);

                entity.Property(t => t.Priority)
                    .HasDefaultValue(TodoPriority.Medium)
                    .HasSentinel(null);

                entity.Property(t => t.CreatedDate)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                entity.Property(t => t.LastModifiedDate)
                    .HasComputedColumnSql("GETUTCDATE()")
                    .ValueGeneratedOnAddOrUpdate();

                // CREATE indexes for better query performance
                entity.HasIndex(t => t.IsComplete)
                    .HasDatabaseName("Todo_IsComplete");

                entity.HasIndex(t => t.Category)
                    .HasDatabaseName("Todo_Category");

                entity.HasIndex(t => new { t.IsComplete, t.DueDate })
                    .HasDatabaseName("Todo_IsComplete_DueDate");

            });
        }
    }
}
