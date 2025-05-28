using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Entity
{
    public class Comment
    {
        public int Id { get; set; }
        public int EntradaId { get; set; }
        public Entry? Entry { get; set; }
        [Required]
        public string Cuerpo { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }
        [Required]
        public string? UsuarioId { get; set; }
        public User? User { get; set; }
        public bool Borrado { get; set; }
        public int? Puntuacion { get; set; }
    }
}
