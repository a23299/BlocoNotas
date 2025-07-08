using System.ComponentModel.DataAnnotations;

namespace BlocoNotas.Models;

public class RegisterVM
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords don't match.")]
    public string ConfirmPassword { get; set; }
}    
