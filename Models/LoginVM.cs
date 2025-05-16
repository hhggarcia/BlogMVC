using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Models
{
    public class LoginVM
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo debe ser un correo valido")]
        public required string Email { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name = "Recuerdame")]
        public bool Recuerdame { get; set; }

        public string UrlRetorno { get; set; }
    }
}
