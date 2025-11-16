using TodoWebApp.Server.Models;

namespace TodoWebApp.Server.DTOs 
{
    public class CreateTodoDto 
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public TodoPriority Priority { get; set; } = TodoPriority.Medium;
        public DateTime? DueDate { get; set; }
        public string? Category { get; set; }
        public string? Tags { get; set; }
    }
}
