using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models
{
    /// <summary>
    /// Modelo para capturar dados do formulário de login.
    /// </summary>
    public class Login
    {
        /// <summary>
        /// Email do utilizador que será usado para autenticação.
        /// Campo obrigatório e deve ser um endereço de email válido.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        /// <summary>
        /// Palavra-passe do utilizador.
        /// Campo obrigatório e deve ser tratado como senha (escondido no formulário).
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}