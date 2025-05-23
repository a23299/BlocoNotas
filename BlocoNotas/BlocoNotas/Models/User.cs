using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BlocoNotas.Models;
public class User
{
    /// <summary>
    /// ID do user
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Nome do User
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UserName { get; set; }
    
    /// <summary>
    /// Email do User
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    /// <summary>
    /// Password do User
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 4)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    /// <summary>
    /// Quando o User foi criado
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Notas do User
    public ICollection<Note> Notes { get; set; }
    
    // Notas de outros Users partilhados com o User
    public ICollection<NoteShare> SharedWithMe { get; set; }
}