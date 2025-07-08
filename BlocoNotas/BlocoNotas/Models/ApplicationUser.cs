using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models
{
    /// <summary>
    /// Modelo personalizado de utilizador para ASP.NET Identity
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? NomeCompleto { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relação 1:n com as notas criadas por este utilizador
        public ICollection<Note> Notes { get; set; } = new List<Note>();

        // Relação 1:n com partilhas de notas que este utilizador recebeu
        public ICollection<NoteShare> SharedWithMe { get; set; } = new List<NoteShare>();
    }
}