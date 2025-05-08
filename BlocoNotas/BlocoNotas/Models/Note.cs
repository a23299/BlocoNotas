using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models;
public class Note
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; }
    
    public string Content { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;
    
    // Foreign key
    public string UserId { get; set; }
    
    // Navigation properties
    [Required]
    public User User { get; set; }
    
    public ICollection<NoteTag> NoteTags { get; set; }
    public ICollection<NoteShare> SharedWith { get; set; }
}