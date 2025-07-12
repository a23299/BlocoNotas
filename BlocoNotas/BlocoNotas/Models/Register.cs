using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models
{
    /// <summary>
    /// Modelo usado para o registo de novos utilizadores.
    /// </summary>
    public class Register
    {
        /// <summary>
        /// Nome de utilizador.
        /// </summary>
        [Required]
        public string UserName { get; set; }
        
        /// <summary>
        /// Endereço de email do utilizador.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        /// <summary>
        /// Palavra-passe do utilizador.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        /// <summary>
        /// Confirmação da palavra-passe, deve coincidir com Password.
        /// </summary>
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords don't match.")]
        public string ConfirmPassword { get; set; }
    }
}