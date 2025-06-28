# 🎵 PlaylistPilot

A modern, feature-rich console application for managing your music playlists built with .NET 8 and Entity Framework Core.

![Screenshot 2025-06-28 185254](https://github.com/user-attachments/assets/19fd252d-fd9a-45ef-a63b-142739877baa)

## 🚀 Features

### Core Functionality
- ✅ **Song Management**: Add, list, update, and delete songs with metadata
- ✅ **Playlist Management**: Create and manage playlists with descriptions
- ✅ **Playlist-Song Relations**: Add/remove songs to/from playlists with ordering
- ✅ **Advanced Reordering**: Move songs within playlists and between playlists
- ✅ **Rich Statistics**: Comprehensive analytics and reporting

### Technical Features
- 🎨 **Modern CLI Interface** powered by Spectre.Console
- 🗄️ **Entity Framework Core 8.0.17** with SQL Server
- 🏗️ **Dependency Injection** using Microsoft.Extensions.Hosting
- 📊 **Rich Data Visualization** with tables, panels, and progress bars
- 🔍 **Duplicate Detection** and data integrity features
- 📈 **Growth Trends** and usage analytics

## 🛠️ Technology Stack

- **.NET 8.0** - Target framework
- **C# 12.0** - Programming language
- **Entity Framework Core 8.0.17** - ORM and data access
- **SQL Server LocalDB** - Database engine
- **Spectre.Console** - Modern console UI framework
- **Microsoft.Extensions.Hosting** - Dependency injection and hosting

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## 🚀 Getting Started

### 1. Clone the Repository


### 2. Install Dependencies


### 3. Set up the Database
The application uses Entity Framework Core migrations. Run the following commands:


### 4. Run the Application


## 📖 Usage Guide

### First Launch
When you first run PlaylistPilot, you'll see the welcome banner and main menu:



### Managing Songs
1. **Add Song**: Enter song details (title, artist, album, genre, duration)
2. **List Songs**: View all songs in a formatted table
3. **Update Song**: Modify existing song information
4. **Delete Song**: Remove songs (with confirmation)

### Managing Playlists  
1. **Add Playlist**: Create new playlists with name and description
2. **List Playlists**: View all playlists with song counts and durations
3. **View Details**: See detailed playlist information with song list
4. **Update Playlist**: Edit playlist metadata
5. **Delete Playlist**: Remove playlists (with confirmation)

### Playlist-Song Management
1. **Add Song to Playlist**: Select playlist and song to add
2. **Remove Song**: Remove songs from playlists with auto-reordering
3. **Reorder Songs**: Change song positions within playlists
4. **Move Between Playlists**: Transfer songs from one playlist to another
5. **View by Playlist**: See all playlists and their songs

### Statistics & Analytics
1. **Database Overview**: High-level statistics and recent activity
2. **Song Statistics**: Duration analysis, genre distribution, top artists
3. **Playlist Statistics**: Playlist analysis and rankings
4. **Top Artists & Genres**: Leaderboards with visual rankings
5. **Detailed Reports**: Advanced analytics including:
   - Songs added by date (with visual charts)
   - Duration distribution analysis
   - Duplicate detection
   - Growth trends (last 30 days)

## 🏗️ Project Structure

![Screenshot 2025-06-28 185551](https://github.com/user-attachments/assets/c7341a9e-1a9f-432d-97c8-ff06e4d20c37)


## 🗄️ Database Schema

### Tables
- **Songs**: Store song metadata (title, artist, album, genre, duration)
- **Playlists**: Store playlist information (name, description, dates)
- **PlaylistSongs**: Junction table with ordering (playlist-song relationships)

### Key Features
- **Foreign Key Constraints**: Maintain data integrity
- **Cascade Deletes**: Clean up related data automatically
- **Unique Constraints**: Prevent duplicate songs in playlists
- **Indexes**: Optimize common queries (artist, title, playlist names)

## ⚙️ Configuration

### Database Connection
Update `appsettings.json` to customize your database connection:



### Logging
Configure logging levels in `appsettings.json`:



## 🚀 Advanced Features

### Duplicate Detection
PlaylistPilot automatically detects potential duplicate songs based on:
- Case-insensitive title matching
- Case-insensitive artist matching
- Displays duplicate groups with IDs for easy management

### Data Integrity
- **Automatic Reordering**: When songs are removed, remaining songs are automatically reordered
- **Constraint Validation**: Prevents invalid data entry
- **Cascade Deletes**: Maintains referential integrity

### Performance Optimizations
- **Async Operations**: All database operations use async/await
- **Eager Loading**: Efficient data loading with Include statements
- **Indexed Queries**: Database indexes on frequently queried columns

## 🔧 Development

### Adding New Features
1. **Models**: Add new entities in the `Models/` folder
2. **Migrations**: Create migrations for schema changes
3. **Services**: Extend `CliService` with new functionality
4. **UI**: Use Spectre.Console for consistent styling

### Running Migrations
