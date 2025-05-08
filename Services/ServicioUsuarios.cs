using BlogMVC.Entity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlogMVC.Services
{
    public interface IServicioUsuarios
    {
        string? ObtenerUsuarioId();
        Task<bool> PuedeUsuarioHacerCrudComentarios();
        Task<bool> PuedeUsuarioHacerCrudEntradas();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly UserManager<User> _userManager;
        private readonly HttpContext _httpContext;
        private readonly User usuarioActual;
        private static readonly string[] RolesCRUDEntradas =
        {
            Constantes.CRUDEntradas, Constantes.RolAdmin
        };
        private static readonly string[] RolesCRUDComentarios =
        {
            Constantes.BorraComentarios, Constantes.RolAdmin
        };

        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            _userManager = userManager;
            _httpContext = httpContextAccessor.HttpContext!;
            usuarioActual = new User{ Id = ObtenerUsuarioId()!};
        }

        public async Task<bool> PuedeUsuarioHacerCrudEntradas()
        {
            return await UsuarioEstaEnRol(RolesCRUDEntradas);
        }

        public async Task<bool> PuedeUsuarioHacerCrudComentarios()
        {
            return await UsuarioEstaEnRol(RolesCRUDComentarios);
        }

        private async Task<bool> UsuarioEstaEnRol(IEnumerable<string> roles)
        {
            var rolesUsuario = await _userManager.GetRolesAsync(usuarioActual);
            return roles.Any(rolesUsuario.Contains);
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
