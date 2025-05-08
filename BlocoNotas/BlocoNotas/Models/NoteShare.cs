using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models;
public class NoteShare
{
    public int Id { get; set; }
    
    public int NoteId { get; set; }
    
    [Required]
    public Note Note { get; set; }
    
    public int SharedWithUserId { get; set; }
    
    [Required]
    public User SharedWithUser { get; set; }
    
    public DateTime SharedAt { get; set; } = DateTime.Now;
    public bool CanEdit { get; set; } = false;
}