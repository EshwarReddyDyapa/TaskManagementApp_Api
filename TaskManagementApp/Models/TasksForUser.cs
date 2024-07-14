namespace TaskManagementApp.Models
{
    public class TasksForUser
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime DueDate { get; set;}
        public bool IsCompleted { get; set; }
        public int EmployeeId { get; set; }
    }
}
