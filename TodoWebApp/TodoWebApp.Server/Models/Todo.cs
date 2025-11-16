
namespace TodoWebApp.Server.Models 
{
    public class Todo 
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsComplete { get; set; } = false;
        public TodoPriority? Priority { get; set; }
        public DateTime DueDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Tags {  get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    }
}
