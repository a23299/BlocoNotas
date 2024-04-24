using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models
{
    public class Note
    {
        public Note()
        {
            ListaNotasTagged = new HashSet<NoteTagged>();
        }

        /// <summary>
        /// Id das Notas
        /// </summary>
        [Key]
        public int NoteId { get; set; }

        /// <summary>
        /// Id do user que criou a nota - FK
        /// </summary>
        [Required]
        [ForeignKey(nameof(User))]
        public int UserFK { get; set; }
        public User User { get; set; }


        /// <summary>
        /// Título da Nota
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Titulo { get; set; }

        /// <summary>
        /// Conteúdo da Nota
        /// </summary>
        public string? Conteudo { get; set; }

        /// <summary>
        /// Data de tempo de criacao da Nota
        /// </summary>
        public DateTime Criada { get; set; }

        /// <summary>
        /// Data e tempo do último update a Nota 
        /// </summary>
        public DateTime? Updated { get; set; }

        //relacionamento entre Notas e Tags
        public ICollection<NoteTagged> ListaNotasTagged { get; set; }
    }
}
