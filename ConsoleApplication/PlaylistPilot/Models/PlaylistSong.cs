using System.ComponentModel.DataAnnotations;

namespace PlaylistPilot.Models 
{
    public class PlaylistSong {
        public int Id { get; set; }

        public int PlaylistId { get; set; }
        public virtual Playlist Playlist { get; set; } = null!;

        public int SongId { get; set; }
        public virtual Song Song { get; set; } = null!;

        public int Order {  get; set; }
        public DateTime AddedToPlaylistDate { get; set; } = DateTime.UtcNow;
    }
}
