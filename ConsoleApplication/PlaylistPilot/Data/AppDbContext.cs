using Microsoft.EntityFrameworkCore;
using PlaylistPilot.Models;


namespace PlaylistPilot.Data;


public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets for each entity
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<PlaylistSong> PlaylistSongs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // Configure Playlist entity
        modelBuilder.Entity<Playlist>(entity => {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Index for performance on name searches
            entity.HasIndex(p => p.Name);
        });

        // Configure Song entity
        modelBuilder.Entity<Song>(entity => {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Title).IsRequired().HasMaxLength(300);
            entity.Property(s => s.Artist).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Album).HasMaxLength(200);
            entity.Property(s => s.Genre).HasMaxLength(50);
            entity.Property(s => s.AddedDate).HasDefaultValueSql("GETUTCDATE()");

            // Indexes for performance on common searches
            entity.HasIndex(s => s.Artist);
            entity.HasIndex(s => s.Title);
        });

        // Configure PlaylistSong junction table with relationships
        modelBuilder.Entity<PlaylistSong>(entity => {
            entity.HasKey(ps => ps.Id);

            // Configure foreign key relationships
            entity.HasOne(ps => ps.Playlist)
                  .WithMany(p => p.PlaylistSongs)
                  .HasForeignKey(ps => ps.PlaylistId)
                  .OnDelete(DeleteBehavior.Cascade); // Delete playlist songs when playlist is deleted

            entity.HasOne(ps => ps.Song)
                  .WithMany(s => s.PlaylistSongs)
                  .HasForeignKey(ps => ps.SongId)
                  .OnDelete(DeleteBehavior.Cascade); // Delete playlist songs when song is deleted

            entity.Property(ps => ps.AddedToPlaylistDate).HasDefaultValueSql("GETUTCDATE()");

            // Ensure unique song order within each playlist
            entity.HasIndex(ps => new { ps.PlaylistId, ps.Order }).IsUnique();

            // Prevent duplicate songs in the same playlist
            entity.HasIndex(ps => new { ps.PlaylistId, ps.SongId }).IsUnique();
        });
    }
}
