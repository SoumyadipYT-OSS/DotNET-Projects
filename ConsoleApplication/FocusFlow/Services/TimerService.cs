using Spectre.Console;
using FocusFlow.Data;
using FocusFlow.Models;

namespace FocusFlow.Services;

public class TimerService {
    private readonly AppDbContext _db;

    public TimerService(AppDbContext db) {
        _db = db;
    }


    public async Task StartTimerAsync(TimeSpan duration, string label, SessionType type, string taskName = "", string tag = "") {
        var start = DateTime.Now;
        var end = start + duration;

        AnsiConsole.MarkupLine($"[bold green]Starting {label} session for {duration.TotalMinutes} minutes...[/]");

        await AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(new ProgressColumn[]
            {
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new RemainingTimeColumn(),
            new SpinnerColumn()
            })
            .StartAsync(async ctx => {
                var task = ctx.AddTask($"[yellow]{label}[/]", maxValue: duration.TotalSeconds);

                while (!ctx.IsFinished) {
                    var elapsed = DateTime.Now - start;
                    task.Value = Math.Min(elapsed.TotalSeconds, duration.TotalSeconds);
                    await Task.Delay(1000);
                }
            });

        AnsiConsole.MarkupLine($"\n[bold green]✔ {label} session complete![/]");

        // Log to database
        var session = new FocusSession {
            TaskName = taskName,
            Tag = tag,
            StartTime = start,
            EndTime = DateTime.Now,
            Type = type,
            Outcome = SessionOutcome.Completed
        };

        _db.FocusSessions.Add(session);
        await _db.SaveChangesAsync();

        AnsiConsole.MarkupLine("[grey]Session logged to database.[/]");

        // Notification
        try {
            Console.Beep(); // Windows only
        } catch {
            Console.Write("\a"); // Fallback bell
        }

        AnsiConsole.MarkupLine("[bold green]🔔 Time's up![/]");

        // Auto-start next session prompt
        if (type == SessionType.Focus) {
            var startBreak = AnsiConsole.Confirm("Start a short break?");
            if (startBreak) {
                await StartTimerAsync(TimeSpan.FromMinutes(5), "Short Break", SessionType.ShortBreak);
            }
        } else if (type == SessionType.ShortBreak || type == SessionType.LongBreak) {
            var startFocus = AnsiConsole.Confirm("Start another focus session?");
            if (startFocus) {
                var nextTask = AnsiConsole.Ask<string>("Enter task name:");
                var nextTag = AnsiConsole.Ask<string>("Enter tag (e.g., #coding):");
                await StartTimerAsync(TimeSpan.FromMinutes(25), "Focus", SessionType.Focus, nextTask, nextTag);
            }
        }
    }

}
