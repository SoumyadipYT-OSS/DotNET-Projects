using System.ComponentModel.DataAnnotations;

namespace PlaylistPilot.Models;

public class Song {
    public int Id { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Artist { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Album {  get; set; }


    public int DurationSeconds { get; set; }

    [MaxLength(50)]
    public string? Genre { get; set; }

    public DateTime AddedDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastModified {  get; set; }


    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = [];
}
