using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlocoNotas.Models
{
    /// <summary>
    /// Modelo personalizado de utilizador para ASP.NET Identity que estende <see cref="IdentityUser"/>.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Nome completo do utilizador.
        /// </summary>
        [StringLength(100)]
        public string? NomeCompleto { get; set; }

        /// <summary>
        /// Data de criação do utilizador. Valor padrão é a data e hora atuais.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Coleção de notas criadas pelo utilizador.
        /// Relação um-para-muitos.
        /// </summary>
        public ICollection<Note> Notes { get; set; } = new List<Note>();

        /// <summary>
        /// Coleção de partilhas de notas que foram compartilhadas com este utilizador.
        /// Relação um-para-muitos.
        /// </summary>
        public ICollection<NoteShare> SharedWithMe { get; set; } = new List<NoteShare>();

        /// <summary>
        /// Propriedade para armazenar a password temporariamente no input.
        /// Esta propriedade não é mapeada para a base de dados.
        /// </summary>
        [NotMapped]
        public string? Password { get; set; }
    }
}