using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlocoNotas.Models
{
    /// <summary>
    /// Representa uma partilha de uma nota com um utilizador específico, incluindo permissões.
    /// </summary>
    public class NoteShare
    {
        /// <summary>
        /// Identificador único da partilha da nota.
        /// </summary>
        public int NoteShareId { get; set; }
        
        /// <summary>
        /// Identificador da nota que está a ser partilhada.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Note))]
        public int NoteId { get; set; }

        /// <summary>
        /// Referência à nota que está a ser partilhada.
        /// </summary>
        public Note Note { get; set; }
        
        /// <summary>
        /// Identificador do utilizador com quem a nota foi partilhada.
        /// </summary>
        [Required]
        [ForeignKey(nameof(SharedWithUser))]
        public string UserShareFK { get; set; }

        /// <summary>
        /// Referência ao utilizador com quem a nota foi partilhada.
        /// </summary>
        public ApplicationUser SharedWithUser { get; set; }
        
        /// <summary>
        /// Data e hora em que a nota foi partilhada.
        /// </summary>
        public DateTime SharedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Indica se o utilizador com quem a nota foi partilhada tem permissão para editar a nota.
        /// </summary>
        public bool CanEdit { get; set; } = false;
    }
}