# 🎯 FocusFlow

A sleek, terminal-based Pomodoro tracker built with .NET 8, EF Core, and Spectre.Console.  
Track your focus sessions, visualize your progress, and stay in the zone—all from your command line.

---

## 🚀 Features

- ⏱️ Real-time Pomodoro timer with animated progress bar
- 🧠 Session logging with task name, tag, and outcome
- 📊 Weekly summaries and goal tracking
- 📁 Export session history to CSV
- 🔍 Filter sessions by tag
- 🎨 Customizable themes (Ocean, Sunset, Sunrise, etc.)
- 🔔 Auto-start next session with optional prompts
- 💾 Persistent settings via `settings.json`

---

## 🛠️ Tech Stack

- [.NET 8](https://dotnet.microsoft.com/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Spectre.Console](https://spectreconsole.net/)
- SQL Server 2022

---

## 📦 Setup

1. **Clone the repo**  
   ```bash
   git clone https://github.com/yourusername/focusflow.git
   cd focusflow

2. **Configure your database**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FocusFlowDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

3. **Apply migrations**
```Bash
dotnet ef database update
```

4. **Run the app**
```Bash
dotnet run
```

#### Happy Coding!