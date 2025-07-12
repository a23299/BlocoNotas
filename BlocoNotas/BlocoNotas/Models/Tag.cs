using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlocoNotas.Models
{
    /// <summary>
    /// Representa uma tag que pode ser associada a notas.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// ID único da Tag.
        /// </summary>
        public int TagId { get; set; }
        
        /// <summary>
        /// Nome da Tag.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        /// <summary>
        /// Coleção de associações entre esta Tag e as Notas que a possuem.
        /// </summary>
        [JsonIgnore]
        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    }
}