using FocusFlow.Models;
using FocusFlow.Services;
using Spectre.Console;


namespace FocusFlow.CLI;


public class Menu {
    private readonly TimerService _timerService;
    private readonly SessionViewer _viewer;

    public Menu(TimerService timerService, SessionViewer viewer) {
        _timerService = timerService;
        _viewer = viewer;
    }


    public async Task ShowAsync() {
        AnsiConsole.Clear();

        // Startup Banner
        AnsiConsole.Write(
            new FigletText("FocusFlow")
                .Centered()
                .Color(Color.RosyBrown));

        AnsiConsole.MarkupLine("[grey]Your terminal Pomodoro companion[/]\n");

        // Optional Theme Selector
        var theme = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a [green]theme[/]:")
                .AddChoices("Classic", "Olive", "Fuchsia"));

        var primaryColor = theme switch {
            "Olive" => Color.Olive,
            "Fuchsia" => Color.Fuchsia,
            _ => Color.DodgerBlue1
        };

        while (true) {
            AnsiConsole.Clear();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold {primaryColor}]\nWhat would you like to do?[/]")
                    .AddChoices(new[]
                    {
                    "Start Focus Session",
                    "Start Short Break",
                    "Start Long Break",
                    "View Session History",
                    "View Weekly Summary",
                    "Filter by Tag",
                    "Export to CSV",
                    "View Goal Progress",
                    "Exit"
                    }));

            switch (choice) {
                case "Start Focus Session":
                    await StartSessionAsync(SessionType.Focus, TimeSpan.FromMinutes(25));
                    break;

                case "Start Short Break":
                    await StartSessionAsync(SessionType.ShortBreak, TimeSpan.FromMinutes(5));
                    break;

                case "Start Long Break":
                    await StartSessionAsync(SessionType.LongBreak, TimeSpan.FromMinutes(15));
                    break;

                case "View Session History":
                    await _viewer.ShowHistoryAsync();
                    break;

                case "View Weekly Summary":
                    await _viewer.ShowSummaryAsync();
                    break;

                case "Filter by Tag":
                    await _viewer.ShowFilteredHistoryAsync();
                    break;

                case "Export to CSV":
                    await _viewer.ExportToCsvAsync();
                    break;

                case "View Goal Progress":
                    await _viewer.ShowGoalProgressAsync();
                    break;

                case "Exit":
                    AnsiConsole.MarkupLine($"[bold {primaryColor}]Goodbye! Stay focused![/]");
                    return;
            }

            AnsiConsole.MarkupLine("\n[grey]Press any key to return to the menu...[/]");
            Console.ReadKey(true);
        }
    }


    private async Task StartSessionAsync(SessionType type, TimeSpan duration) {
        string label = type switch {
            SessionType.Focus => "Focus",
            SessionType.ShortBreak => "Short Break",
            SessionType.LongBreak => "Long Break",
            _ => "Session"
        };

        string taskName = "";
        string tag = "";

        if (type == SessionType.Focus) {
            taskName = AnsiConsole.Ask<string>("Enter task name:");
            tag = AnsiConsole.Ask<string>("Enter tag (e.g., #coding):");
        }

        await _timerService.StartTimerAsync(duration, label, type, taskName, tag);
    }
}
