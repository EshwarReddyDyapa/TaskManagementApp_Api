namespace TaskManagementApp.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Position { get; set; }
        public int ManagerId { get; set; }
    }
}
