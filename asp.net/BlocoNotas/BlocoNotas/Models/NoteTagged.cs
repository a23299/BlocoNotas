using static Azure.Core.HttpHeader;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models
{
    public class NoteTagged
    {
        /// <summary>
        /// Id da Nota Tagged
        /// </summary>
        [Key]
        public int NoteTaggedId { get; set; }

        /// <summary>
        /// Id da Nota - FK
        /// </summary>
        [ForeignKey(nameof(Note))]
        public int NoteFK { get; set; }
        public required Note Note { get; set; }

        /// <summary>
        /// Id da Tag - FK
        /// </summary>
        [ForeignKey(nameof(Tag))]
        public int TagFK { get; set; }
        public required Tag Tag { get; set; }

    }
}
