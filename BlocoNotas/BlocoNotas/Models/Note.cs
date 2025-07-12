using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BlocoNotas.Models
{
    /// <summary>
    /// Representa uma nota criada pelo utilizador, contendo título, conteúdo, datas e ligações a utilizadores e tags.
    /// </summary>
    public class Note
    {
        /// <summary>
        /// Identificador único da nota.
        /// </summary>
        public int NoteId { get; set; }
        
        /// <summary>
        /// Título da nota.
        /// Campo obrigatório com máximo de 200 caracteres.
        /// </summary>
        [Required]
        [StringLength(200)]
        [Display(Name = "Título")]
        public string Title { get; set; }
        
        /// <summary>
        /// Conteúdo textual da nota.
        /// </summary>
        [Display(Name = "Conteúdo")]
        public string Content { get; set; }
        
        /// <summary>
        /// Data e hora em que a nota foi criada.
        /// </summary>
        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Data e hora da última modificação da nota.
        /// </summary>
        [Display(Name = "Última Atualização")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Indica se a nota foi marcada como eliminada (soft delete).
        /// </summary>
        [Display(Name = "Eliminada")]
        public bool IsDeleted { get; set; } = false;
        
        /// <summary>
        /// Chave estrangeira para o utilizador que criou a nota.
        /// </summary>
        [Display(Name = "Utilizador")]
        //[Required]
        [ForeignKey(nameof(User))]
        public string? UserFK { get; set; } 
        
        /// <summary>
        /// Referência ao utilizador que criou a nota.
        /// Esta propriedade é ignorada na serialização JSON.
        /// </summary>
        [JsonIgnore]
        public ApplicationUser? User { get; set; }
        
        /// <summary>
        /// Coleção de ligações entre esta nota e as suas tags associadas.
        /// </summary>
        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
        
        /// <summary>
        /// Coleção de partilhas desta nota com outros utilizadores.
        /// </summary>
        public ICollection<NoteShare> SharedWith { get; set; } =  new List<NoteShare>();
    }

    /// <summary>
    /// DTO simples para representar um utilizador com ID e nome de utilizador.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Identificador do utilizador.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nome do utilizador.
        /// </summary>
        public string UserName { get; set; }
    }
}
