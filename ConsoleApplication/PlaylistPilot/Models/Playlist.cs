using System.ComponentModel.DataAnnotations;

namespace PlaylistPilot.Models;


public class Playlist {
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastModified {  get; set; }


    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
}
