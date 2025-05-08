using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models;
public class Tag
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; }
    
    // Navigation property
    public ICollection<NoteTag> NoteTags { get; set; }
}