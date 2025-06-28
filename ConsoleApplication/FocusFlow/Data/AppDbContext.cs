using Microsoft.EntityFrameworkCore;
using FocusFlow.Models;

namespace FocusFlow.Data 
{
    public class AppDbContext : DbContext
    {
        public DbSet<FocusSession> FocusSessions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<FocusSession>().ToTable("FocusSessions");
        }
    }
}