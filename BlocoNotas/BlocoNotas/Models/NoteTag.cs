using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BlocoNotas.Models
{
    /// <summary>
    /// Representa a relação entre uma Nota e uma Tag (etiqueta).
    /// Esta classe funciona como tabela de ligação (join table) entre Note e Tag.
    /// </summary>
    public class NoteTag
    {
        /// <summary>
        /// Chave estrangeira para a Nota associada a esta relação.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Note))]
        public int NoteTagFK { get; set; }

        /// <summary>
        /// A Nota associada a esta relação.
        /// </summary>
        [JsonIgnore]
        public Note Note { get; set; }
        
        /// <summary>
        /// Chave estrangeira para a Tag associada a esta relação.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Tag))]
        public int TagFK { get; set; }

        /// <summary>
        /// A Tag associada a esta relação.
        /// </summary>
        public Tag Tag { get; set; }
    }
}