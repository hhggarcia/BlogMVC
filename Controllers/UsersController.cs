using BlogMVC.Data;
using BlogMVC.Entity;
using BlogMVC.Models;
using BlogMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogMVC.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly ApplicationDbContext context;

        public UsersController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            var user = new User()
            {
                Email = registerVM.Email,
                Nombre = registerVM.Nombre,
                UserName = registerVM.Email
            };

            var result = await userManager.CreateAsync(user, password: registerVM.Password);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: true);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(registerVM);
            }
        }

        [AllowAnonymous]
        public IActionResult Login(string? mensaje = null, string? urlRetorno = null)
        {
            if (mensaje is not null)
            {
                ViewData["mensaje"] = mensaje;
            }

            if (urlRetorno is not null)
            {
                ViewData["urlRetorno"] = urlRetorno;
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            var result = await signInManager.PasswordSignInAsync(loginVM.Email, loginVM.Password, loginVM.Recuerdame, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (string.IsNullOrWhiteSpace(loginVM.UrlRetorno))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return LocalRedirect(loginVM.UrlRetorno);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario o password incorrecto.");

                return View(loginVM);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> Listado(string? mensaje = null)
        {
            var usuarios = await context.Users.Select(x => new UserVM()
            {
                Id = x.Id,
                Email = x.Email!
            }).ToListAsync();

            var modelo = new UserListVM()
            {
                Users = usuarios,
                Mensaje = mensaje
            };

            return View(modelo);
        }

        [HttpGet]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> RolesUsuario(string id)
        {
            var usuario = await userManager.FindByIdAsync(id);

            if (usuario is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var rolesUser = await userManager.GetRolesAsync(usuario);
            var rolesExists = await context.Roles.ToListAsync();

            var rolesDelUser = rolesExists.Select(x => new UserRoleVM
            {
                Name = x.Name!,
                IsCheck = rolesUser.Contains(x.Name!)
            });

            var modelo = new UsersRolesUserVM()
            {
                UserId = id,
                Email = usuario.Email!,
                Roles = rolesDelUser.OrderBy(x => x.Name)
            };

            return View(modelo);
        }

        [HttpPost]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> EditRoles(EditRolesVM modelo)
        {
            var usuario = await userManager.FindByIdAsync(modelo.UserId);

            if (usuario is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await context.UserRoles.Where(x => x.UserId == usuario.Id).ExecuteDeleteAsync();
            await userManager.AddToRolesAsync(usuario, modelo.RolesSelects);

            var mensaje = $"Los roles de {usuario.Email} han sido actualizados";

            return RedirectToAction("Listado", new { mensaje });
        }
    }
}
