using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BlocoNotas.Models;

namespace BlocoNotas.Models;
public class NoteTag
{
    /// <summary>
    /// Nota que tem a tag
    /// </summary>
    [Required]
    [ForeignKey(nameof(Note))]
    public int NoteTagFK { get; set; }
    [JsonIgnore]
    public Note Note { get; set; }
    
    /// <summary>
    /// Tag que está na Nota
    /// </summary>
    [Required]
    [ForeignKey(nameof(Tag))]
    public int TagFK { get; set; }
    public Tag Tag { get; set; }
}