using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace BlocoNotas.Models;
public class NoteShare
{
    /// <summary>
    /// ID do partilhar da Nota
    /// </summary>
    public int NoteShareId { get; set; }
    
    /// <summary>
    /// Nota partilhada
    /// </summary>
    [Required]
    
    [ForeignKey(nameof(Note))]
    public int NoteShareFK { get; set; }
    public Note Note { get; set; }
    
    /// <summary>
    /// User que a Nota foi partilhada
    /// </summary>
    [Required]
    [ForeignKey(nameof(SharedWithUser))]
    
    public int UserShareFK { get; set; }
    
    public User SharedWithUser { get; set; }
    
    /// <summary>
    /// Quando a Nota foi partilhada
    /// </summary>
    public DateTime SharedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Se o User que a Nota foi partilhada pode-a modificar
    /// </summary>
    public bool CanEdit { get; set; } = false;
}