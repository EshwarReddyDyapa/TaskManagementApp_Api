namespace TaskManagementApp.Models
{
    public class AddEmployeeDto
    {
        public required string Name { get; set; }
        public required string Position { get; set; }
        public int ManagerId { get; set; }
    }
}
