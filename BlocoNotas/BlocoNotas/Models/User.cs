using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BlocoNotas.Models;
public class User
{
    public string Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; }
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; }
    
    public bool IsAdmin { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Navigation properties
    public ICollection<Note> Notes { get; set; }
    public ICollection<NoteShare> SharedWithMe { get; set; }
}