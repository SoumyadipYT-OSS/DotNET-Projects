using Microsoft.EntityFrameworkCore;
using PlaylistPilot.Data;
using PlaylistPilot.Models;
using Spectre.Console;

namespace PlaylistPilot.Services;

/// <summary>
/// Main CLI service that handles user interactions and commands
/// </summary>
public class CliService : ICliService {
    private readonly AppDbContext _context;

    public CliService(AppDbContext context) {
        _context = context;
    }

    public async Task RunAsync() {
        // Display welcome banner
        DisplayWelcomeBanner();

        var exit = false;
        while (!exit) {
            try {
                var choice = ShowMainMenu();
                exit = await HandleMenuChoice(choice);
            } catch (Exception ex) {
                AnsiConsole.WriteException(ex);
                AnsiConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        AnsiConsole.MarkupLine("[green]Thank you for using PlaylistPilot! 🎵[/]");
    }

    private void DisplayWelcomeBanner() {
        var panel = new Panel(new FigletText("PlaylistPilot").Centered().Color(Color.Purple)) {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Purple)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.MarkupLine("[dim]Your personal music playlist manager[/]");
        AnsiConsole.WriteLine();
    }

    private string ShowMainMenu() {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .PageSize(10)
                .AddChoices(new[] {
                    "🎵 Manage Songs",
                    "📝 Manage Playlists",
                    "🔗 Manage Playlist Songs",
                    "📊 View Statistics",
                    "🚪 Exit"
                }));
    }

    private async Task<bool> HandleMenuChoice(string choice) {
        return choice switch {
            "🎵 Manage Songs" => await HandleSongMenu(),
            "📝 Manage Playlists" => await HandlePlaylistMenu(),
            "🔗 Manage Playlist Songs" => await HandlePlaylistSongMenu(),
            "📊 View Statistics" => await ShowStatistics(),
            "🚪 Exit" => true,
            _ => false
        };
    }

    private async Task<bool> HandleSongMenu() {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Song Management")
                .AddChoices(new[] {
                    "Add Song",
                    "List Songs",
                    "Update Song",
                    "Delete Song",
                    "Back to Main Menu"
                }));

        switch (choice) {
            case "Add Song":
                await AddSong();
                break;
            case "List Songs":
                await ListSongs();
                break;
            case "Update Song":
                await UpdateSong();
                break;
            case "Delete Song":
                await DeleteSong();
                break;
            case "Back to Main Menu":
                return false;
        }

        return false;
    }

    #region Song Management Methods

    private async Task AddSong() {
        AnsiConsole.MarkupLine("[bold yellow]Add New Song[/]");

        var title = AnsiConsole.Ask<string>("Song [green]title[/]:");
        var artist = AnsiConsole.Ask<string>("Song [green]artist[/]:");
        var album = AnsiConsole.Ask<string>("Album (optional):", "");
        var genre = AnsiConsole.Ask<string>("Genre (optional):", "");

        var durationMinutes = AnsiConsole.Ask<int>("Duration in [green]minutes[/]:");
        var durationSeconds = AnsiConsole.Ask<int>("Additional [green]seconds[/] (0-59):", 0);

        var totalSeconds = (durationMinutes * 60) + durationSeconds;

        var song = new Song {
            Title = title,
            Artist = artist,
            Album = string.IsNullOrWhiteSpace(album) ? null : album,
            Genre = string.IsNullOrWhiteSpace(genre) ? null : genre,
            DurationSeconds = totalSeconds
        };

        _context.Songs.Add(song);
        await _context.SaveChangesAsync();

        AnsiConsole.MarkupLine($"[green]✓[/] Song '[bold]{title}[/]' by [bold]{artist}[/] added successfully!");
        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private async Task ListSongs() {
        AnsiConsole.MarkupLine("[bold yellow]All Songs[/]");

        await AnsiConsole.Status()
            .Start("Loading songs...", async ctx => {
                var songs = await _context.Songs
                    .OrderBy(s => s.Artist)
                    .ThenBy(s => s.Title)
                    .ToListAsync();

                if (!songs.Any()) {
                    AnsiConsole.MarkupLine("[red]No songs found.[/]");
                    return;
                }

                var table = new Table();
                table.AddColumn("ID");
                table.AddColumn("Title");
                table.AddColumn("Artist");
                table.AddColumn("Album");
                table.AddColumn("Duration");
                table.AddColumn("Genre");

                foreach (var song in songs) {
                    var duration = TimeSpan.FromSeconds(song.DurationSeconds);
                    var durationStr = $"{duration.Minutes:D2}:{duration.Seconds:D2}";

                    table.AddRow(
                        song.Id.ToString(),
                        song.Title,
                        song.Artist,
                        song.Album ?? "-",
                        durationStr,
                        song.Genre ?? "-"
                    );
                }

                AnsiConsole.Write(table);
            });

        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private async Task UpdateSong() {
        AnsiConsole.MarkupLine("[bold yellow]Update Song[/]");

        var songId = AnsiConsole.Ask<int>("Enter song [green]ID[/] to update:");

        var song = await _context.Songs.FindAsync(songId);
        if (song == null) {
            AnsiConsole.MarkupLine("[red]Song not found.[/]");
            Console.ReadKey();
            return;
        }

        AnsiConsole.MarkupLine($"Updating: [bold]{song.Title}[/] by [bold]{song.Artist}[/]");

        var title = AnsiConsole.Ask("New title:", song.Title);
        var artist = AnsiConsole.Ask("New artist:", song.Artist);
        var album = AnsiConsole.Ask("New album:", song.Album ?? "");
        var genre = AnsiConsole.Ask("New genre:", song.Genre ?? "");

        song.Title = title;
        song.Artist = artist;
        song.Album = string.IsNullOrWhiteSpace(album) ? null : album;
        song.Genre = string.IsNullOrWhiteSpace(genre) ? null : genre;
        song.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        AnsiConsole.MarkupLine("[green]✓ Song updated successfully![/]");
        Console.ReadKey();
    }

    private async Task DeleteSong() {
        AnsiConsole.MarkupLine("[bold yellow]Delete Song[/]");

        var songId = AnsiConsole.Ask<int>("Enter song [green]ID[/] to delete:");

        var song = await _context.Songs.FindAsync(songId);
        if (song == null) {
            AnsiConsole.MarkupLine("[red]Song not found.[/]");
            Console.ReadKey();
            return;
        }

        var confirm = AnsiConsole.Confirm($"Are you sure you want to delete '[red]{song.Title}[/]' by '[red]{song.Artist}[/]'?");
        if (confirm) {
            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
            AnsiConsole.MarkupLine("[green]✓ Song deleted successfully![/]");
        } else {
            AnsiConsole.MarkupLine("[yellow]Delete cancelled.[/]");
        }

        Console.ReadKey();
    }

    #endregion

    #region Playlist Management Methods

    private async Task<bool> HandlePlaylistMenu() {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Playlist Management")
                .AddChoices(new[] {
                    "Add Playlist",
                    "List Playlists",
                    "View Playlist Details",
                    "Update Playlist",
                    "Delete Playlist",
                    "Back to Main Menu"
                }));

        switch (choice) {
            case "Add Playlist":
                await AddPlaylist();
                break;
            case "List Playlists":
                await ListPlaylists();
                break;
            case "View Playlist Details":
                await ViewPlaylistDetails();
                break;
            case "Update Playlist":
                await UpdatePlaylist();
                break;
            case "Delete Playlist":
                await DeletePlaylist();
                break;
            case "Back to Main Menu":
                return false;
        }

        return false;
    }

    private async Task AddPlaylist() {
        AnsiConsole.MarkupLine("[bold yellow]Create New Playlist[/]");

        var name = AnsiConsole.Ask<string>("Playlist [green]name[/]:");
        var description = AnsiConsole.Ask<string>("Description (optional):", "");

        var playlist = new Playlist {
            Name = name,
            Description = string.IsNullOrWhiteSpace(description) ? null : description
        };

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();

        AnsiConsole.MarkupLine($"[green]✓[/] Playlist '[bold]{name}[/]' created successfully!");
        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private async Task ListPlaylists() {
        AnsiConsole.MarkupLine("[bold yellow]All Playlists[/]");

        await AnsiConsole.Status()
            .Start("Loading playlists...", async ctx => {
                var playlists = await _context.Playlists
                    .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                if (!playlists.Any()) {
                    AnsiConsole.MarkupLine("[red]No playlists found.[/]");
                    return;
                }

                var table = new Table();
                table.AddColumn("ID");
                table.AddColumn("Name");
                table.AddColumn("Description");
                table.AddColumn("Songs");
                table.AddColumn("Total Duration");
                table.AddColumn("Created");

                foreach (var playlist in playlists) {
                    var songCount = playlist.PlaylistSongs.Count;
                    var totalSeconds = playlist.PlaylistSongs.Sum(ps => ps.Song.DurationSeconds);
                    var totalDuration = TimeSpan.FromSeconds(totalSeconds);
                    var durationStr = $"{totalDuration.Hours:D2}:{totalDuration.Minutes:D2}:{totalDuration.Seconds:D2}";

                    table.AddRow(
                        playlist.Id.ToString(),
                        playlist.Name,
                        playlist.Description ?? "-",
                        songCount.ToString(),
                        durationStr,
                        playlist.CreatedDate?.ToString("yyyy-MM-dd") ?? "-"
                    );
                }

                AnsiConsole.Write(table);
            });

        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private async Task ViewPlaylistDetails() {
        AnsiConsole.MarkupLine("[bold yellow]Playlist Details[/]");

        var playlistId = AnsiConsole.Ask<int>("Enter playlist [green]ID[/] to view:");

        var playlist = await _context.Playlists
            .Include(p => p.PlaylistSongs)
            .ThenInclude(ps => ps.Song)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist == null) {
            AnsiConsole.MarkupLine("[red]Playlist not found.[/]");
            Console.ReadKey();
            return;
        }

        // Display playlist header
        var panel = new Panel($"""
            [bold yellow]{playlist.Name}[/]
            [dim]{playlist.Description ?? "No description"}[/]
            
            [green]Created:[/] {playlist.CreatedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Unknown"}
            [green]Songs:[/] {playlist.PlaylistSongs.Count}
            [green]Total Duration:[/] {TimeSpan.FromSeconds(playlist.PlaylistSongs.Sum(ps => ps.Song.DurationSeconds)):hh\:mm\:ss}
            """) {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Blue)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        if (playlist.PlaylistSongs.Any()) {
            AnsiConsole.MarkupLine("[bold]Songs in this playlist:[/]");

            var table = new Table();
            table.AddColumn("#");
            table.AddColumn("Title");
            table.AddColumn("Artist");
            table.AddColumn("Album");
            table.AddColumn("Duration");

            foreach (var playlistSong in playlist.PlaylistSongs.OrderBy(ps => ps.Order)) {
                var duration = TimeSpan.FromSeconds(playlistSong.Song.DurationSeconds);
                var durationStr = $"{duration.Minutes:D2}:{duration.Seconds:D2}";

                table.AddRow(
                    playlistSong.Order.ToString(),
                    playlistSong.Song.Title,
                    playlistSong.Song.Artist,
                    playlistSong.Song.Album ?? "-",
                    durationStr
                );
            }

            AnsiConsole.Write(table);
        } else {
            AnsiConsole.MarkupLine("[dim]This playlist is empty.[/]");
        }

        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private async Task UpdatePlaylist() {
        AnsiConsole.MarkupLine("[bold yellow]Update Playlist[/]");

        var playlistId = AnsiConsole.Ask<int>("Enter playlist [green]ID[/] to update:");

        var playlist = await _context.Playlists.FindAsync(playlistId);
        if (playlist == null) {
            AnsiConsole.MarkupLine("[red]Playlist not found.[/]");
            Console.ReadKey();
            return;
        }

        AnsiConsole.MarkupLine($"Updating: [bold]{playlist.Name}[/]");

        var name = AnsiConsole.Ask("New name:", playlist.Name);
        var description = AnsiConsole.Ask("New description:", playlist.Description ?? "");

        playlist.Name = name;
        playlist.Description = string.IsNullOrWhiteSpace(description) ? null : description;
        playlist.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        AnsiConsole.MarkupLine("[green]✓ Playlist updated successfully![/]");
        Console.ReadKey();
    }

    private async Task DeletePlaylist() {
        AnsiConsole.MarkupLine("[bold yellow]Delete Playlist[/]");

        var playlistId = AnsiConsole.Ask<int>("Enter playlist [green]ID[/] to delete:");

        var playlist = await _context.Playlists
            .Include(p => p.PlaylistSongs)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist == null) {
            AnsiConsole.MarkupLine("[red]Playlist not found.[/]");
            Console.ReadKey();
            return;
        }

        var songCount = playlist.PlaylistSongs.Count;
        var confirmMessage = songCount > 0
            ? $"Are you sure you want to delete '[red]{playlist.Name}[/]' and remove [red]{songCount}[/] song(s) from it?"
            : $"Are you sure you want to delete '[red]{playlist.Name}[/]'?";

        var confirm = AnsiConsole.Confirm(confirmMessage);
        if (confirm) {
            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            AnsiConsole.MarkupLine("[green]✓ Playlist deleted successfully![/]");
        } else {
            AnsiConsole.MarkupLine("[yellow]Delete cancelled.[/]");
        }

        Console.ReadKey();
    }

    #endregion

    #region Playlist-Song Management Methods

    private async Task<bool> HandlePlaylistSongMenu() {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Playlist-Song Management")
                .AddChoices(new[] {
                    "Add Song to Playlist",
                    "Remove Song from Playlist",
                    "Reorder Songs in Playlist",
                    "Move Song Between Playlists",
                    "View Songs by Playlist",
                    "Back to Main Menu"
                }));

        switch (choice) {
            case "Add Song to Playlist":
                await AddSongToPlaylist();
                break;
            case "Remove Song from Playlist":
                await RemoveSongFromPlaylist();
                break;
            case "Reorder Songs in Playlist":
                await ReorderSongsInPlaylist();
                break;
            case "Move Song Between Playlists":
                await MoveSongBetweenPlaylists();
                break;
            case "View Songs by Playlist":
                await ViewSongsByPlaylist();
                break;
            case "Back to Main Menu":
                return false;
        }

        return false;
    }

    private async Task AddSongToPlaylist() {
        AnsiConsole.MarkupLine("[bold yellow]Add Song to Playlist[/]");

        // Select playlist
        var playlists = await _context.Playlists.OrderBy(p => p.Name).ToListAsync();
        if (!playlists.Any()) {
            AnsiConsole.MarkupLine("[red]No playlists available. Create a playlist first.[/]");
            Console.ReadKey();
            return;
        }

        var playlistChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [green]playlist[/]:")
                .AddChoices(playlists.Select(p => $"{p.Id}: {p.Name}").ToArray()));

        var playlistId = int.Parse(playlistChoice.Split(':')[0]);

        // Select song
        var songs = await _context.Songs.OrderBy(s => s.Artist).ThenBy(s => s.Title).ToListAsync();
        if (!songs.Any()) {
            AnsiConsole.MarkupLine("[red]No songs available. Add songs first.[/]");
            Console.ReadKey();
            return;
        }

        var songChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [green]song[/]:")
                .PageSize(10)
                .AddChoices(songs.Select(s => $"{s.Id}: {s.Title} - {s.Artist}").ToArray()));

        var songId = int.Parse(songChoice.Split(':')[0]);

        // Check if song is already in playlist
        var existingEntry = await _context.PlaylistSongs
            .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

        if (existingEntry != null) {
            AnsiConsole.MarkupLine("[red]This song is already in the selected playlist.[/]");
            Console.ReadKey();
            return;
        }

        // Get next order number
        var maxOrder = await _context.PlaylistSongs
            .Where(ps => ps.PlaylistId == playlistId)
            .MaxAsync(ps => (int?)ps.Order) ?? 0;

        var playlistSong = new PlaylistSong {
            PlaylistId = playlistId,
            SongId = songId,
            Order = maxOrder + 1
        };

        _context.PlaylistSongs.Add(playlistSong);
        await _context.SaveChangesAsync();

        var selectedSong = songs.First(s => s.Id == songId);
        var selectedPlaylist = playlists.First(p => p.Id == playlistId);

        AnsiConsole.MarkupLine($"[green]✓[/] Added '[bold]{selectedSong.Title}[/]' to playlist '[bold]{selectedPlaylist.Name}[/]' at position {maxOrder + 1}");
        Console.ReadKey();
    }

    private async Task RemoveSongFromPlaylist() {
        AnsiConsole.MarkupLine("[bold yellow]Remove Song from Playlist[/]");

        // Select playlist
        var playlistsWithSongs = await _context.Playlists
            .Include(p => p.PlaylistSongs)
            .Where(p => p.PlaylistSongs.Any())
            .OrderBy(p => p.Name)
            .ToListAsync();

        if (!playlistsWithSongs.Any()) {
            AnsiConsole.MarkupLine("[red]No playlists with songs found.[/]");
            Console.ReadKey();
            return;
        }

        var playlistChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [green]playlist[/]:")
                .AddChoices(playlistsWithSongs.Select(p => $"{p.Id}: {p.Name} ({p.PlaylistSongs.Count} songs)").ToArray()));

        var playlistId = int.Parse(playlistChoice.Split(':')[0]);

        // Get songs in the selected playlist
        var playlistSongs = await _context.PlaylistSongs
            .Include(ps => ps.Song)
            .Where(ps => ps.PlaylistId == playlistId)
            .OrderBy(ps => ps.Order)
            .ToListAsync();

        var songChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [red]song to remove[/]:")
                .AddChoices(playlistSongs.Select(ps => $"{ps.Order}: {ps.Song.Title} - {ps.Song.Artist}").ToArray()));

        var orderToRemove = int.Parse(songChoice.Split(':')[0]);
        var playlistSongToRemove = playlistSongs.First(ps => ps.Order == orderToRemove);

        var confirm = AnsiConsole.Confirm($"Remove '[red]{playlistSongToRemove.Song.Title}[/]' from the playlist?");
        if (confirm) {
            _context.PlaylistSongs.Remove(playlistSongToRemove);

            // Reorder remaining songs
            var remainingSongs = playlistSongs.Where(ps => ps.Order > orderToRemove).ToList();
            foreach (var ps in remainingSongs) {
                ps.Order--;
            }

            await _context.SaveChangesAsync();
            AnsiConsole.MarkupLine("[green]✓ Song removed and playlist reordered successfully![/]");
        } else {
            AnsiConsole.MarkupLine("[yellow]Removal cancelled.[/]");
        }

        Console.ReadKey();
    }

    private async Task ReorderSongsInPlaylist() {
        AnsiConsole.MarkupLine("[bold yellow]Reorder Songs in Playlist[/]");

        // Select playlist with songs
        var playlistsWithSongs = await _context.Playlists
            .Include(p => p.PlaylistSongs)
            .ThenInclude(ps => ps.Song)
            .Where(p => p.PlaylistSongs.Count > 1)
            .OrderBy(p => p.Name)
            .ToListAsync();

        if (!playlistsWithSongs.Any()) {
            AnsiConsole.MarkupLine("[red]No playlists with multiple songs found.[/]");
            Console.ReadKey();
            return;
        }

        var playlistChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [green]playlist[/] to reorder:")
                .AddChoices(playlistsWithSongs.Select(p => $"{p.Id}: {p.Name} ({p.PlaylistSongs.Count} songs)").ToArray()));

        var playlistId = int.Parse(playlistChoice.Split(':')[0]);
        var selectedPlaylist = playlistsWithSongs.First(p => p.Id == playlistId);

        // Show current order
        AnsiConsole.MarkupLine($"[bold]Current order in '{selectedPlaylist.Name}':[/]");
        var currentSongs = selectedPlaylist.PlaylistSongs.OrderBy(ps => ps.Order).ToList();

        var table = new Table();
        table.AddColumn("Position");
        table.AddColumn("Song");
        table.AddColumn("Artist");

        foreach (var ps in currentSongs) {
            table.AddRow(ps.Order.ToString(), ps.Song.Title, ps.Song.Artist);
        }
        AnsiConsole.Write(table);

        // Select song to move
        var songToMoveChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [yellow]song to move[/]:")
                .AddChoices(currentSongs.Select(ps => $"{ps.Order}: {ps.Song.Title} - {ps.Song.Artist}").ToArray()));

        var currentPosition = int.Parse(songToMoveChoice.Split(':')[0]);
        var newPosition = AnsiConsole.Ask<int>($"Enter new position (1-{currentSongs.Count}):");

        if (newPosition < 1 || newPosition > currentSongs.Count) {
            AnsiConsole.MarkupLine("[red]Invalid position.[/]");
            Console.ReadKey();
            return;
        }

        if (currentPosition == newPosition) {
            AnsiConsole.MarkupLine("[yellow]Song is already in that position.[/]");
            Console.ReadKey();
            return;
        }

        // Reorder logic
        var songToMove = currentSongs.First(ps => ps.Order == currentPosition);

        if (currentPosition < newPosition) {
            // Moving down: shift songs up
            var songsToShift = currentSongs.Where(ps => ps.Order > currentPosition && ps.Order <= newPosition).ToList();
            foreach (var ps in songsToShift) {
                ps.Order--;
            }
        } else {
            // Moving up: shift songs down
            var songsToShift = currentSongs.Where(ps => ps.Order >= newPosition && ps.Order < currentPosition).ToList();
            foreach (var ps in songsToShift) {
                ps.Order++;
            }
        }

        songToMove.Order = newPosition;
        await _context.SaveChangesAsync();

        AnsiConsole.MarkupLine($"[green]✓[/] Moved '[bold]{songToMove.Song.Title}[/]' to position {newPosition}");
        Console.ReadKey();
    }

    private async Task MoveSongBetweenPlaylists() {
        AnsiConsole.MarkupLine("[bold yellow]Move Song Between Playlists[/]");
        AnsiConsole.MarkupLine("[dim]This will remove the song from one playlist and add it to another.[/]");

        // This is a combination of remove and add operations
        AnsiConsole.MarkupLine("[bold]Step 1: Remove from current playlist[/]");
        await RemoveSongFromPlaylist();

        AnsiConsole.MarkupLine("[bold]Step 2: Add to new playlist[/]");
        await AddSongToPlaylist();
    }

    private async Task ViewSongsByPlaylist() {
        AnsiConsole.MarkupLine("[bold yellow]Songs by Playlist[/]");

        var playlists = await _context.Playlists
            .Include(p => p.PlaylistSongs)
            .ThenInclude(ps => ps.Song)
            .OrderBy(p => p.Name)
            .ToListAsync();

        if (!playlists.Any()) {
            AnsiConsole.MarkupLine("[red]No playlists found.[/]");
            Console.ReadKey();
            return;
        }

        foreach (var playlist in playlists) {
            var panel = new Panel($"[bold yellow]{playlist.Name}[/] ({playlist.PlaylistSongs.Count} songs)") {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Blue)
            };
            AnsiConsole.Write(panel);

            if (playlist.PlaylistSongs.Any()) {
                var table = new Table();
                table.AddColumn("#");
                table.AddColumn("Title");
                table.AddColumn("Artist");
                table.AddColumn("Duration");

                foreach (var ps in playlist.PlaylistSongs.OrderBy(ps => ps.Order)) {
                    var duration = TimeSpan.FromSeconds(ps.Song.DurationSeconds);
                    table.AddRow(
                        ps.Order.ToString(),
                        ps.Song.Title,
                        ps.Song.Artist,
                        $"{duration.Minutes:D2}:{duration.Seconds:D2}"
                    );
                }
                AnsiConsole.Write(table);
            } else {
                AnsiConsole.MarkupLine("[dim]  (Empty playlist)[/]");
            }
            AnsiConsole.WriteLine();
        }

        Console.ReadKey();
    }

    #endregion

    // Placeholder for statistics
    #region Statistics Methods

    private async Task<bool> ShowStatistics() {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Statistics & Reports")
                .AddChoices(new[] {
                "📊 Database Overview",
                "🎵 Song Statistics",
                "📝 Playlist Statistics",
                "🏆 Top Artists & Genres",
                "📈 Detailed Reports",
                "Back to Main Menu"
                }));

        switch (choice) {
            case "📊 Database Overview":
                await ShowDatabaseOverview();
                break;
            case "🎵 Song Statistics":
                await ShowSongStatistics();
                break;
            case "📝 Playlist Statistics":
                await ShowPlaylistStatistics();
                break;
            case "🏆 Top Artists & Genres":
                await ShowTopArtistsAndGenres();
                break;
            case "📈 Detailed Reports":
                await ShowDetailedReports();
                break;
            case "Back to Main Menu":
                return false;
        }

        return false;
    }

    private async Task ShowDatabaseOverview() {
        AnsiConsole.MarkupLine("[bold yellow]Database Overview[/]");

        await AnsiConsole.Status()
            .Start("Calculating statistics...", async ctx => {
                // Get basic counts
                var totalSongs = await _context.Songs.CountAsync();
                var totalPlaylists = await _context.Playlists.CountAsync();
                var totalPlaylistSongs = await _context.PlaylistSongs.CountAsync();

                // Get duration statistics
                var totalDurationSeconds = await _context.Songs.SumAsync(s => (long)s.DurationSeconds);
                var totalDuration = TimeSpan.FromSeconds(totalDurationSeconds);

                var avgDurationSeconds = totalSongs > 0 ? await _context.Songs.AverageAsync(s => (double)s.DurationSeconds) : 0;
                var avgDuration = TimeSpan.FromSeconds(avgDurationSeconds);

                // Get playlist statistics
                var avgSongsPerPlaylist = totalPlaylists > 0 ? (double)totalPlaylistSongs / totalPlaylists : 0;

                // Recent additions
                var songsAddedToday = await _context.Songs
                    .CountAsync(s => s.AddedDate.Date == DateTime.UtcNow.Date);

                var playlistsCreatedToday = await _context.Playlists
                    .CountAsync(p => p.CreatedDate.HasValue && p.CreatedDate.Value.Date == DateTime.UtcNow.Date);

                // Create overview panel
                var overviewPanel = new Panel($"""
                [bold green]📊 Total Songs:[/] {totalSongs:N0}
                [bold blue]📝 Total Playlists:[/] {totalPlaylists:N0}
                [bold cyan]🔗 Total Playlist Entries:[/] {totalPlaylistSongs:N0}
                
                [bold yellow]⏱️  Total Music Duration:[/] {totalDuration:hh\:mm\:ss}
                [bold yellow]📏 Average Song Length:[/] {avgDuration:mm\:ss}
                [bold purple]📊 Avg Songs per Playlist:[/] {avgSongsPerPlaylist:F1}
                
                [bold lime]🆕 Songs Added Today:[/] {songsAddedToday}
                [bold lime]🆕 Playlists Created Today:[/] {playlistsCreatedToday}
                """) {
                    Header = new PanelHeader(" 🎵 PlaylistPilot Database Overview "),
                    Border = BoxBorder.Double,
                    BorderStyle = new Style(Color.Green)
                };

                AnsiConsole.Write(overviewPanel);
            });

        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private async Task ShowSongStatistics() {
        AnsiConsole.MarkupLine("[bold yellow]Song Statistics[/]");

        await AnsiConsole.Status()
            .Start("Analyzing songs...", async ctx => {
                var songs = await _context.Songs.ToListAsync();

                if (!songs.Any()) {
                    AnsiConsole.MarkupLine("[red]No songs found.[/]");
                    return;
                }

                // Duration analysis
                var durations = songs.Select(s => s.DurationSeconds).ToList();
                var totalDuration = TimeSpan.FromSeconds(durations.Sum());
                var avgDuration = TimeSpan.FromSeconds(durations.Average());
                var minDuration = TimeSpan.FromSeconds(durations.Min());
                var maxDuration = TimeSpan.FromSeconds(durations.Max());

                // Genre distribution
                var genreStats = songs
                    .GroupBy(s => s.Genre ?? "Unknown")
                    .Select(g => new { Genre = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .ToList();

                // Artist distribution
                var artistStats = songs
                    .GroupBy(s => s.Artist)
                    .Select(g => new { Artist = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(10)
                    .ToList();

                // Display duration statistics
                var durationPanel = new Panel($"""
                [bold yellow]Total Duration:[/] {totalDuration:hh\:mm\:ss}
                [bold yellow]Average Duration:[/] {avgDuration:mm\:ss}
                [bold yellow]Shortest Song:[/] {minDuration:mm\:ss}
                [bold yellow]Longest Song:[/] {maxDuration:mm\:ss}
                """) {
                    Header = new PanelHeader(" ⏱️ Duration Statistics "),
                    Border = BoxBorder.Rounded
                };
                AnsiConsole.Write(durationPanel);

                // Display genre distribution
                if (genreStats.Any()) {
                    AnsiConsole.MarkupLine("\n[bold]🎭 Genre Distribution:[/]");
                    var genreTable = new Table();
                    genreTable.AddColumn("Genre");
                    genreTable.AddColumn("Count");
                    genreTable.AddColumn("Percentage");

                    foreach (var genre in genreStats.Take(10)) {
                        var percentage = (double)genre.Count / songs.Count * 100;
                        genreTable.AddRow(
                            genre.Genre,
                            genre.Count.ToString(),
                            $"{percentage:F1}%"
                        );
                    }
                    AnsiConsole.Write(genreTable);
                }

                // Display top artists
                if (artistStats.Any()) {
                    AnsiConsole.MarkupLine("\n[bold]🎤 Top 10 Artists:[/]");
                    var artistTable = new Table();
                    artistTable.AddColumn("Rank");
                    artistTable.AddColumn("Artist");
                    artistTable.AddColumn("Songs");

                    for (int i = 0; i < artistStats.Count; i++) {
                        var artist = artistStats[i];
                        artistTable.AddRow(
                            (i + 1).ToString(),
                            artist.Artist,
                            artist.Count.ToString()
                        );
                    }
                    AnsiConsole.Write(artistTable);
                }
            });

        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private async Task ShowPlaylistStatistics() {
        AnsiConsole.MarkupLine("[bold yellow]Playlist Statistics[/]");

        await AnsiConsole.Status()
            .Start("Analyzing playlists...", async ctx => {
                var playlists = await _context.Playlists
                    .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                    .ToListAsync();

                if (!playlists.Any()) {
                    AnsiConsole.MarkupLine("[red]No playlists found.[/]");
                    return;
                }

                // Basic statistics
                var totalPlaylists = playlists.Count;
                var playlistsWithSongs = playlists.Count(p => p.PlaylistSongs.Any());
                var emptyPlaylists = totalPlaylists - playlistsWithSongs;

                // Song count statistics
                var songCounts = playlists.Select(p => p.PlaylistSongs.Count).Where(c => c > 0).ToList();
                var avgSongsPerPlaylist = songCounts.Any() ? songCounts.Average() : 0;
                var maxSongsInPlaylist = songCounts.Any() ? songCounts.Max() : 0;
                var minSongsInPlaylist = songCounts.Any() ? songCounts.Min() : 0;

                // Duration statistics for playlists with songs
                var playlistDurations = playlists
                    .Where(p => p.PlaylistSongs.Any())
                    .Select(p => TimeSpan.FromSeconds(p.PlaylistSongs.Sum(ps => ps.Song.DurationSeconds)))
                    .ToList();

                var totalPlaylistDuration = playlistDurations.Any()
                    ? TimeSpan.FromSeconds(playlistDurations.Sum(d => d.TotalSeconds))
                    : TimeSpan.Zero;

                var avgPlaylistDuration = playlistDurations.Any()
                    ? TimeSpan.FromSeconds(playlistDurations.Average(d => d.TotalSeconds))
                    : TimeSpan.Zero;

                // Display statistics
                var statsPanel = new Panel($"""
                [bold green]📊 Total Playlists:[/] {totalPlaylists}
                [bold blue]🎵 Playlists with Songs:[/] {playlistsWithSongs}
                [bold red]📭 Empty Playlists:[/] {emptyPlaylists}
                
                [bold yellow]📏 Avg Songs per Playlist:[/] {avgSongsPerPlaylist:F1}
                [bold yellow]🔝 Most Songs in Playlist:[/] {maxSongsInPlaylist}
                [bold yellow]🔻 Least Songs in Playlist:[/] {(songCounts.Any() ? minSongsInPlaylist : 0)}
                
                [bold purple]⏱️  Total Playlist Duration:[/] {totalPlaylistDuration:hh\:mm\:ss}
                [bold purple]📊 Avg Playlist Duration:[/] {avgPlaylistDuration:hh\:mm\:ss}
                """) {
                    Header = new PanelHeader(" 📝 Playlist Analysis "),
                    Border = BoxBorder.Double,
                    BorderStyle = new Style(Color.Blue)
                };

                AnsiConsole.Write(statsPanel);

                // Show longest and shortest playlists
                if (playlists.Any(p => p.PlaylistSongs.Any())) {
                    AnsiConsole.MarkupLine("\n[bold]🏆 Playlist Rankings:[/]");

                    var rankingTable = new Table();
                    rankingTable.AddColumn("Playlist");
                    rankingTable.AddColumn("Songs");
                    rankingTable.AddColumn("Duration");
                    rankingTable.AddColumn("Created");

                    var topPlaylists = playlists
                        .Where(p => p.PlaylistSongs.Any())
                        .OrderByDescending(p => p.PlaylistSongs.Count)
                        .Take(5)
                        .ToList();

                    foreach (var playlist in topPlaylists) {
                        var duration = TimeSpan.FromSeconds(playlist.PlaylistSongs.Sum(ps => ps.Song.DurationSeconds));
                        rankingTable.AddRow(
                            playlist.Name,
                            playlist.PlaylistSongs.Count.ToString(),
                            $"{duration:hh\\:mm\\:ss}",
                            playlist.CreatedDate?.ToString("yyyy-MM-dd") ?? "Unknown"
                        );
                    }

                    AnsiConsole.Write(rankingTable);
                }
            });

        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private async Task ShowTopArtistsAndGenres() {
        AnsiConsole.MarkupLine("[bold yellow]Top Artists & Genres[/]");

        await AnsiConsole.Status()
            .Start("Generating rankings...", async ctx => {
                var songs = await _context.Songs.ToListAsync();

                if (!songs.Any()) {
                    AnsiConsole.MarkupLine("[red]No songs found.[/]");
                    return;
                }

                // Top Artists by song count
                var topArtists = songs
                    .GroupBy(s => s.Artist)
                    .Select(g => new {
                        Artist = g.Key,
                        SongCount = g.Count(),
                        TotalDuration = TimeSpan.FromSeconds(g.Sum(s => s.DurationSeconds))
                    })
                    .OrderByDescending(a => a.SongCount)
                    .Take(10)
                    .ToList();

                // Top Genres
                var topGenres = songs
                    .GroupBy(s => s.Genre ?? "Unknown")
                    .Select(g => new {
                        Genre = g.Key,
                        SongCount = g.Count(),
                        TotalDuration = TimeSpan.FromSeconds(g.Sum(s => s.DurationSeconds))
                    })
                    .OrderByDescending(g => g.SongCount)
                    .Take(10)
                    .ToList();

                // Display top artists
                if (topArtists.Any()) {
                    var artistPanel = new Panel("Top Artists by Song Count") {
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(Color.Green)
                    };
                    AnsiConsole.Write(artistPanel);

                    var artistTable = new Table();
                    artistTable.AddColumn("Rank");
                    artistTable.AddColumn("Artist");
                    artistTable.AddColumn("Songs");
                    artistTable.AddColumn("Total Duration");

                    for (int i = 0; i < topArtists.Count; i++) {
                        var artist = topArtists[i];
                        var medal = i switch {
                            0 => "🥇",
                            1 => "🥈",
                            2 => "🥉",
                            _ => (i + 1).ToString()
                        };

                        artistTable.AddRow(
                            medal,
                            artist.Artist,
                            artist.SongCount.ToString(),
                            $"{artist.TotalDuration:hh\\:mm\\:ss}"
                        );
                    }
                    AnsiConsole.Write(artistTable);
                }

                AnsiConsole.WriteLine();

                // Display top genres
                if (topGenres.Any()) {
                    var genrePanel = new Panel("Top Genres by Song Count") {
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(Color.Purple)
                    };
                    AnsiConsole.Write(genrePanel);

                    var genreTable = new Table();
                    genreTable.AddColumn("Rank");
                    genreTable.AddColumn("Genre");
                    genreTable.AddColumn("Songs");
                    genreTable.AddColumn("Percentage");
                    genreTable.AddColumn("Total Duration");

                    for (int i = 0; i < topGenres.Count; i++) {
                        var genre = topGenres[i];
                        var percentage = (double)genre.SongCount / songs.Count * 100;
                        var medal = i switch {
                            0 => "🥇",
                            1 => "🥈",
                            2 => "🥉",
                            _ => (i + 1).ToString()
                        };

                        genreTable.AddRow(
                            medal,
                            genre.Genre,
                            genre.SongCount.ToString(),
                            $"{percentage:F1}%",
                            $"{genre.TotalDuration:hh\\:mm\\:ss}"
                        );
                    }
                    AnsiConsole.Write(genreTable);
                }
            });

        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private async Task ShowDetailedReports() {
        AnsiConsole.MarkupLine("[bold yellow]Detailed Reports[/]");

        var reportChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a detailed report:")
                .AddChoices(new[] {
                "📅 Songs Added by Date",
                "📊 Duration Distribution",
                "🔍 Find Duplicates",
                "📈 Growth Trends",
                "Back"
                }));

        switch (reportChoice) {
            case "📅 Songs Added by Date":
                await ShowSongsByDate();
                break;
            case "📊 Duration Distribution":
                await ShowDurationDistribution();
                break;
            case "🔍 Find Duplicates":
                await FindPotentialDuplicates();
                break;
            case "📈 Growth Trends":
                await ShowGrowthTrends();
                break;
        }
    }

    private async Task ShowSongsByDate() {
        AnsiConsole.MarkupLine("[bold yellow]Songs Added by Date[/]");

        var songs = await _context.Songs.ToListAsync();

        if (!songs.Any()) {
            AnsiConsole.MarkupLine("[red]No songs found.[/]");
            Console.ReadKey();
            return;
        }

        var songsByDate = songs
            .GroupBy(s => s.AddedDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Date)
            .Take(14) // Last 14 days
            .ToList();

        var table = new Table();
        table.AddColumn("Date");
        table.AddColumn("Songs Added");
        table.AddColumn("Visual");

        foreach (var day in songsByDate) {
            var bar = new string('█', Math.Min(day.Count, 20));
            table.AddRow(
                day.Date.ToString("yyyy-MM-dd"),
                day.Count.ToString(),
                $"[green]{bar}[/]"
            );
        }

        AnsiConsole.Write(table);
        Console.ReadKey();
    }

    private async Task ShowDurationDistribution() {
        AnsiConsole.MarkupLine("[bold yellow]Song Duration Distribution[/]");

        var songs = await _context.Songs.ToListAsync();

        if (!songs.Any()) {
            AnsiConsole.MarkupLine("[red]No songs found.[/]");
            Console.ReadKey();
            return;
        }

        var durationRanges = new[]
        {
        ("0-1 min", 0, 60),
        ("1-2 min", 60, 120),
        ("2-3 min", 120, 180),
        ("3-4 min", 180, 240),
        ("4-5 min", 240, 300),
        ("5-6 min", 300, 360),
        ("6+ min", 360, int.MaxValue)
    };

        var table = new Table();
        table.AddColumn("Duration Range");
        table.AddColumn("Count");
        table.AddColumn("Percentage");
        table.AddColumn("Visual");

        foreach (var (label, min, max) in durationRanges) {
            var count = songs.Count(s => s.DurationSeconds >= min && s.DurationSeconds < max);
            var percentage = (double)count / songs.Count * 100;
            var bar = new string('█', Math.Min((int)(percentage / 2), 50));

            table.AddRow(
                label,
                count.ToString(),
                $"{percentage:F1}%",
                $"[blue]{bar}[/]"
            );
        }

        AnsiConsole.Write(table);
        Console.ReadKey();
    }

    private async Task FindPotentialDuplicates() {
        AnsiConsole.MarkupLine("[bold yellow]Potential Duplicate Songs[/]");

        var songs = await _context.Songs.ToListAsync();

        if (!songs.Any()) {
            AnsiConsole.MarkupLine("[red]No songs found.[/]");
            Console.ReadKey();
            return;
        }

        var duplicates = songs
            .GroupBy(s => new { Title = s.Title.ToLower().Trim(), Artist = s.Artist.ToLower().Trim() })
            .Where(g => g.Count() > 1)
            .ToList();

        if (!duplicates.Any()) {
            AnsiConsole.MarkupLine("[green]No potential duplicates found![/]");
        } else {
            AnsiConsole.MarkupLine($"[yellow]Found {duplicates.Count} potential duplicate groups:[/]");

            var table = new Table();
            table.AddColumn("Title");
            table.AddColumn("Artist");
            table.AddColumn("Duplicate Count");
            table.AddColumn("IDs");

            foreach (var group in duplicates) {
                var ids = string.Join(", ", group.Select(s => s.Id));
                table.AddRow(
                    group.Key.Title,
                    group.Key.Artist,
                    group.Count().ToString(),
                    ids
                );
            }

            AnsiConsole.Write(table);
        }

        Console.ReadKey();
    }

    private async Task ShowGrowthTrends() {
        AnsiConsole.MarkupLine("[bold yellow]Growth Trends (Last 30 Days)[/]");

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var songGrowth = await _context.Songs
            .Where(s => s.AddedDate >= thirtyDaysAgo)
            .GroupBy(s => s.AddedDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync();

        var playlistGrowth = await _context.Playlists
            .Where(p => p.CreatedDate.HasValue && p.CreatedDate >= thirtyDaysAgo)
            .GroupBy(p => p.CreatedDate!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync();

        if (!songGrowth.Any() && !playlistGrowth.Any()) {
            AnsiConsole.MarkupLine("[yellow]No recent activity in the last 30 days.[/]");
        } else {
            var growthPanel = new Panel($"""
            [bold green]📈 Songs Added (30 days):[/] {songGrowth.Sum(s => s.Count)}
            [bold blue]📈 Playlists Created (30 days):[/] {playlistGrowth.Sum(p => p.Count)}
            [bold yellow]📊 Most Active Day (Songs):[/] {(songGrowth.Any() ? songGrowth.OrderByDescending(s => s.Count).First().Date.ToString("yyyy-MM-dd") : "None")}
            [bold purple]📊 Most Active Day (Playlists):[/] {(playlistGrowth.Any() ? playlistGrowth.OrderByDescending(p => p.Count).First().Date.ToString("yyyy-MM-dd") : "None")}
            """) {
                Header = new PanelHeader(" 📈 Growth Summary "),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(growthPanel);
        }

        Console.ReadKey();
    }

    #endregion
}