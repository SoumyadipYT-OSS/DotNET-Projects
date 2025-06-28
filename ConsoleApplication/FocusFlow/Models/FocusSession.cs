
namespace FocusFlow.Models 
{
    public class FocusSession {
        public int Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;     // e.g., #coding #mathematics #physics
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public SessionType Type { get; set; }    // Focus, ShortBreak, LongBreak
        public SessionOutcome Outcome { get; set; }     // Completed, Skipped, Interrupted
    }
}
