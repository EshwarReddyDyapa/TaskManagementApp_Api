namespace TaskManagementApp.Models
{
    public class Note
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public int TaskId { get; set; }
        public required string PathToDoc { get; set; }
    }
}
