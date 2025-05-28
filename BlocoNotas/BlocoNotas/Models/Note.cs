using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlocoNotas.Models;
public class Note
{
    /// <summary>
    /// ID da Nota
    /// </summary>
    public int NoteId { get; set; }
    
    /// <summary>
    /// Título da Nota
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; }
    
    /// <summary>
    /// Conteúdo da Nota
    /// </summary>
    public string Content { get; set; }
    
    /// <summary>
    /// Quando a nota foi criada
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Quando a Nota foi modificada pela última vez
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Se a Nota foi eliminada 
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// User da Nota
    /// </summary>
    [Required]
    [ForeignKey(nameof(User))]
    public int UserFK { get; set; }
    public User User { get; set; }
    
    // Tags da Nota
    public ICollection<NoteTag> NoteTags { get; set; } = [];
    
    // Users que a Nota foi partilhada
    public ICollection<NoteShare> SharedWith { get; set; } = [];
}