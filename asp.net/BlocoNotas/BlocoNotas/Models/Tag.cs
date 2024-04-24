using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models
{
    public class Tag
    {
        public Tag()
        {
            ListaNotasTagged = new HashSet<NoteTagged>();
        }

        /// <summary>
        /// Id da Tag
        /// </summary>
        [Key]
        public int TagId { get; set; }

        /// <summary>
        /// Nome da Tag
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        // relacionamento entre Notas e Tags
        public ICollection<NoteTagged> ListaNotasTagged { get; set; }
    }
}
