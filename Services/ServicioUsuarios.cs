using BlogMVC.Entity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlogMVC.Services
{
    public interface IServicioUsuarios
    {
        string? ObtenerUsuarioId();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly UserManager<User> _userManager;
        private readonly HttpContext _httpContext;

        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            _userManager = userManager;
            _httpContext = httpContextAccessor.HttpContext!;
        }

        public string? ObtenerUsuarioId()
        {
            var idClaim = _httpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (idClaim is null)
            {
                return null;
            }

            return idClaim.Value;
        }
    }
}
