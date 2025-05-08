using BlogMVC.Data;
using BlogMVC.Entity;
using BlogMVC.Models;
using BlogMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogMVC.Controllers
{
    public class EntradasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAlmacenadorArchivo _almacenarArchivos;
        private readonly IServicioUsuarios _serviceUsuarios;
        private readonly string contenedor = "entradas";
        public EntradasController(ApplicationDbContext context,
            IAlmacenadorArchivo almacenarArchivos,
            IServicioUsuarios serviceUsuarios)
        {
            _context = context;
            _almacenarArchivos = almacenarArchivos;
            _serviceUsuarios = serviceUsuarios;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var entry = await _context.Entries
                .Include(c => c.UsuarioCreacion)
                .Include(c => c.Comments)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync();

            if (entry is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var puedeEditar = await _serviceUsuarios.PuedeUsuarioHacerCrudEntradas();
            var model = new EntryDetailsVM()
            {
                Id = entry.Id,
                Titulo = entry.Titulo,
                Cuerpo = entry.Cuerpo,
                PortadaUrl = entry.PortadaUrl,
                FechaPublicacion = entry.FechaPublicacion,
                EscritoPor = entry.UsuarioCreacion!.Nombre,
                MostrarBotonEdicion = puedeEditar
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = $"{Constantes.RolAdmin}, {Constantes.CRUDEntradas}")]
        public async Task<IActionResult> Crear(EntryCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? portadaUrl = null;

            if (model.ImagenPortada is not null)
            {
                portadaUrl = await _almacenarArchivos.Almacenar(contenedor, model.ImagenPortada);

                string usuarioId = _serviceUsuarios.ObtenerUsuarioId()!;

                var entry = new Entry()
                {
                    Titulo = model.Titulo,
                    Cuerpo = model.Cuerpo,
                    FechaPublicacion = DateTime.UtcNow,
                    PortadaUrl = portadaUrl,
                    UsuarioCreacionId = usuarioId,
                };

                _context.Add(entry);
                await _context.SaveChangesAsync();

                return RedirectToAction("Detalle", new { id = entry.Id });
            }

            return View(model);
        }
    }
}
