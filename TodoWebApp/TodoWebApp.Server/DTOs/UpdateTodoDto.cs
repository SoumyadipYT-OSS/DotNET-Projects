using TodoWebApp.Server.Models;

namespace TodoWebApp.Server.DTOs 
{
    public class UpdateTodoDto 
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TodoPriority? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Category { get; set; }
        public string? Tags { get; set; }
        public bool? IsComplete { get; set; }
    }
}
