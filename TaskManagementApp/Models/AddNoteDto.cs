namespace TaskManagementApp.Models
{
    public class AddNoteDto
    {
        public required string Content { get; set; }
        public int TaskId { get; set; }
        public required string PathToDoc { get; set; }
    }
}
