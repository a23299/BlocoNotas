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
    [Display(Name = "Título")]
    public string Title { get; set; }
    
    /// <summary>
    /// Conteúdo da Nota
    /// </summary>
    [Display(Name = "Conteúdo")]
    public string Content { get; set; }
    
    /// <summary>
    /// Quando a nota foi criada
    /// </summary>
    [Display(Name = "Data de Criação")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Quando a Nota foi modificada pela última vez
    /// </summary>
    [Display(Name = "Última Atualização")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Se a Nota foi eliminada 
    /// </summary>
    [Display(Name = "Eliminada")]
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// User da Nota
    /// </summary>
    [Display(Name = "Utilizador")]
    [Required]
    [ForeignKey(nameof(User))]
    public int UserFK { get; set; }
    public User User { get; set; }
    
    // Tags da Nota
    public ICollection<NoteTag> NoteTags { get; set; } = [];
    
    // Users que a Nota foi partilhada
    public ICollection<NoteShare> SharedWith { get; set; } = [];
}