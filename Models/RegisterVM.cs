using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Models
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo debe ser un correo valido")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public required string Nombre { get; set; }
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
