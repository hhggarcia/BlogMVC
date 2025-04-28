using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Entity
{
    public class User : IdentityUser
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;
    }
}
