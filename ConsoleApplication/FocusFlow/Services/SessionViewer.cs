using FocusFlow.Data;
using FocusFlow.Models;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace FocusFlow.Services;

public class SessionViewer 
{
    private readonly AppDbContext _db;

    // Hardcoded code for define a simple goal model, later update store them in a config file or database.
    private const int DailyGoalMinutes = 100;   // e.g., 4 Pomodoros
    private const int WeeklyGoalMinutes = 500;  // e.g., 20 Pomodoros

    public SessionViewer(AppDbContext db) {
        _db = db;
    }


    public async Task ShowHistoryAsync() {
        var sessions = await _db.FocusSessions
            .OrderByDescending(s => s.StartTime)
            .Take(20)
            .ToListAsync();

        if (!sessions.Any()) {
            AnsiConsole.MarkupLine("[grey]No sessions found.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold underline green]Session History[/]")
            .AddColumn("Date")
            .AddColumn("Type")
            .AddColumn("Task")
            .AddColumn("Tag")
            .AddColumn("Duration")
            .AddColumn("Outcome");

        foreach (var s in sessions) {
            table.AddRow(
                s.StartTime.ToString("g"),
                s.Type.ToString(),
                s.TaskName,
                s.Tag,
                $"{s.Duration.TotalMinutes:F1} min",
                s.Outcome.ToString()
            );
        }

        AnsiConsole.Write(table);
    }


    public async Task ShowSummaryAsync() {
        var now = DateTime.Now;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek); // Sunday
        var sessions = await _db.FocusSessions
            .Where(s => s.StartTime >= weekStart)
            .ToListAsync();

        if (!sessions.Any()) {
            AnsiConsole.MarkupLine("[grey]No sessions found for this week.[/]");
            return;
        }

        var totalFocusTime = sessions
            .Where(s => s.Type == SessionType.Focus && s.Outcome == SessionOutcome.Completed)
            .Sum(s => s.Duration.TotalMinutes);

        var grouped = sessions
            .GroupBy(s => s.Type)
            .Select(g => new {
                Type = g.Key,
                Count = g.Count(),
                TotalMinutes = g.Sum(s => s.Duration.TotalMinutes)
            });

        var table = new Table()
            .Title("[bold underline green]Weekly Summary[/]")
            .AddColumn("Session Type")
            .AddColumn("Count")
            .AddColumn("Total Time (min)");

        foreach (var g in grouped) {
            table.AddRow(g.Type.ToString(), g.Count.ToString(), g.TotalMinutes.ToString("F1"));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[bold green]Total Focus Time:[/] {totalFocusTime:F1} minutes");
    }


    public async Task ShowFilteredHistoryAsync() {
        var tag = AnsiConsole.Ask<string>("Enter tag to filter by (e.g., #coding):");

        var sessions = await _db.FocusSessions
            .Where(s => s.Tag == tag)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        if (!sessions.Any()) {
            AnsiConsole.MarkupLine($"[grey]No sessions found with tag '{tag}'.[/]");
            return;
        }

        var table = new Table()
            .Title($"[bold underline green]Sessions Tagged: {tag}[/]")
            .AddColumn("Date")
            .AddColumn("Type")
            .AddColumn("Task")
            .AddColumn("Duration")
            .AddColumn("Outcome");

        foreach (var s in sessions) {
            table.AddRow(
                s.StartTime.ToString("g"),
                s.Type.ToString(),
                s.TaskName,
                $"{s.Duration.TotalMinutes:F1} min",
                s.Outcome.ToString()
            );
        }

        AnsiConsole.Write(table);
    }


    public async Task ExportToCsvAsync() {
        var sessions = await _db.FocusSessions
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        if (!sessions.Any()) {
            AnsiConsole.MarkupLine("[grey]No sessions to export.[/]");
            return;
        }

        var fileName = $"FocusFlow_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        using var writer = new StreamWriter(fileName);
        writer.WriteLine("Date,Type,Task,Tag,Duration (min),Outcome");

        foreach (var s in sessions) {
            writer.WriteLine($"{s.StartTime:g},{s.Type},{s.TaskName},{s.Tag},{s.Duration.TotalMinutes:F1},{s.Outcome}");
        }

        AnsiConsole.MarkupLine($"[green]✔ Exported to:[/] {fileName}");
    }


    public async Task ShowGoalProgressAsync() {
        var now = DateTime.Now;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek); // Sunday

        var sessions = await _db.FocusSessions
            .Where(s => s.Type == SessionType.Focus && s.Outcome == SessionOutcome.Completed)
            .ToListAsync();

        var todayMinutes = sessions
            .Where(s => s.StartTime.Date == today)
            .Sum(s => s.Duration.TotalMinutes);

        var weekMinutes = sessions
            .Where(s => s.StartTime >= weekStart)
            .Sum(s => s.Duration.TotalMinutes);

        AnsiConsole.MarkupLine("[bold underline green]🎯 Goal Progress[/]\n");

        AnsiConsole.Write(new BarChart()
            .Width(60)
            .Label("[green bold]Focus Time (min)[/]")
            .CenterLabel()
            .AddItem("Today", (float)todayMinutes, todayMinutes >= DailyGoalMinutes ? Color.Green : Color.Yellow)
            .AddItem("This Week", (float)weekMinutes, weekMinutes >= WeeklyGoalMinutes ? Color.Green : Color.Yellow));

        AnsiConsole.MarkupLine($"\n[bold]Daily Goal:[/] {DailyGoalMinutes} min — [bold]Weekly Goal:[/] {WeeklyGoalMinutes} min");
    }

}