using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models
{
    public class User
    {
        public User()
        {
            ListaNotas = new HashSet<Note>();
        }

        /// <summary>
        /// Id do User
        /// </summary>
        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// Nome do User
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// Email do User
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Password do user
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        // relacionamento entre users e notas
        public ICollection<Note> ListaNotas { get; set; }

    }
}
