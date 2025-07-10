using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlocoNotas.Models;
public class Tag
{
    /// <summary>
    /// ID da Tag
    /// </summary>
    public int TagId { get; set; }
    
    /// <summary>
    /// Nome da Tag
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; }
    
    // Notas que têm a Tag
    [JsonIgnore]
    public ICollection<NoteTag> NoteTags { get; set; } = [];
}