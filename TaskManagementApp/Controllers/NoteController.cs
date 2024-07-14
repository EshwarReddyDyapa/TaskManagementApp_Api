using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public NoteController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("GetAllNotes")]
        public async Task<IActionResult> GetAllNotes()
        {
            var notes = await dbContext.Notes.ToListAsync();
            return Ok(notes);
        }

        [HttpPost("CreateNote")]
        public async Task<IActionResult> CreateNote(AddNoteDto addNoteDto)
        {
            var noteEntity = new Note()
            {
                Content = addNoteDto.Content,
                TaskId = addNoteDto.TaskId,
                PathToDoc = addNoteDto.PathToDoc
            };

            await dbContext.Notes.AddAsync(noteEntity);
            await dbContext.SaveChangesAsync();

            return Ok(noteEntity);
        }

        [HttpGet("GetNoteById/{id:int}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var note = await dbContext.Notes.FindAsync(id);
            if(note == null)
            {
                return NotFound(JsonSerializer.Serialize("NotFound"));
            }
            return Ok(note);
        }

        [HttpPut("UpdateNote/{id:int}")]
        public async Task<IActionResult> UpdateNote(int id, AddNoteDto noteDto)
        {
            var note = dbContext.Notes.Find(id);
            if(note == null)
                return NotFound(JsonSerializer.Serialize("No Note found"));

            note.Content = noteDto.Content;
            note.TaskId = noteDto.TaskId;
            note.PathToDoc = noteDto.PathToDoc;

            await dbContext.SaveChangesAsync();

            return Ok(note);
        }

        [HttpDelete("DeleteNote/{id:int}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await dbContext.Notes.FindAsync(id);
            if(note == null)
                return BadRequest(JsonSerializer.Serialize("No Note Found"));

            dbContext.Notes.Remove(note);
            await dbContext.SaveChangesAsync();

            return Ok(JsonSerializer.Serialize("Note Deleted"));
        }
    }
}
