using BlogMVC.Data;
using BlogMVC.Entity;
using BlogMVC.Models;
using BlogMVC.Services;
using BlogMVC.Utils;
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
        private readonly IServicioChat _servicioChat;
        private readonly string contenedor = "entradas";
        public EntradasController(ApplicationDbContext context,
            IAlmacenadorArchivo almacenarArchivos,
            IServicioUsuarios serviceUsuarios,
            IServicioChat servicioChat)
        {
            _context = context;
            _almacenarArchivos = almacenarArchivos;
            _serviceUsuarios = serviceUsuarios;
            _servicioChat = servicioChat;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var entry = await _context.Entries
                .IgnoreQueryFilters()
                .Include(c => c.UsuarioCreacion)
                .Include(c => c.Comments)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

            if (entry is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var puedeEditar = await _serviceUsuarios.PuedeUsuarioHacerCrudEntradas();

            if (entry.Borrado && !puedeEditar)
            {
                var urlRetorno = HttpContext.ObtenerUrlRetorno();
                return RedirectToAction("Login", "Usuarios", new { urlRetorno });
            }

            var puedeBorrarComentarios = await _serviceUsuarios.PuedeUsuarioHacerCrudComentarios();

            var usuarioId = _serviceUsuarios.ObtenerUsuarioId();

            var model = new EntryDetailsVM()
            {
                Id = entry.Id,
                Titulo = entry.Titulo,
                Cuerpo = entry.Cuerpo,
                PortadaUrl = entry.PortadaUrl,
                FechaPublicacion = entry.FechaPublicacion,
                EscritoPor = entry.UsuarioCreacion!.Nombre,
                MostrarBotonEdicion = puedeEditar,
                EntradaBorrada = entry.Borrado,
                Comentarios = entry.Comments.Select(c => new CommentVM()
                {
                    Id = c.Id,
                    Cuerpo = c.Cuerpo,
                    EscritoPor = c.User!.Nombre,
                    FechaPublicacion = c.FechaPublicacion,
                    MostrarBorrar = puedeBorrarComentarios || usuarioId == c.UsuarioId,
                })
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

                return RedirectToAction("Details", new { id = entry.Id });
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = $"{Constantes.RolAdmin}, {Constantes.CRUDEntradas}")]
        public async Task<IActionResult> Edit(int id)
        {
            var entrada = await _context.Entries
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entrada is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var model = new EntryEditVM()
            {
                Id = entrada.Id,
                Titulo = entrada.Titulo,
                Cuerpo = entrada.Cuerpo,
                ImagenPortadaActual = entrada.PortadaUrl
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = $"{Constantes.RolAdmin}, {Constantes.CRUDEntradas}")]
        public async Task<IActionResult> Edit(EntryEditVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entradaDb = await _context.Entries.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == model.Id);

            if (entradaDb is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            string? portadaUrl = null;

            if (model.ImagenPortada is not null)
            {
                portadaUrl = await _almacenarArchivos.Editar(model.ImagenPortadaActual, contenedor, model.ImagenPortada);
            } else if (model.ImagenRemovida)
            {
                await _almacenarArchivos.Borrar(model.ImagenPortadaActual, contenedor);
            }
            else
            {
                portadaUrl = entradaDb.PortadaUrl;
            }

            string usuarioId = _serviceUsuarios.ObtenerUsuarioId()!;

            entradaDb.Titulo = model.Titulo;
            entradaDb.Cuerpo = model.Cuerpo;
            entradaDb.PortadaUrl = portadaUrl;
            entradaDb.UsuarioActualizacionId = usuarioId;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = entradaDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = $"{Constantes.RolAdmin},{Constantes.CRUDEntradas}")]
        public async Task<IActionResult> Borrar(int id,  bool borrado)
        {
            var entradaDb = await _context.Entries.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);

            if (entradaDb is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            entradaDb.Borrado = borrado;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = entradaDb.Id });
        }

        [HttpGet]
        public async Task GenerarCuerpo([FromQuery] string titulo)
        {
            if (string.IsNullOrEmpty(titulo))
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await Response.WriteAsync("El titulo no puede estar vacio");
                return;
            }

            await foreach (var segmento in _servicioChat.GenerarCuerpoStream(titulo))
            {
                await Response.WriteAsync(segmento);
                await Response.Body.FlushAsync();
            }
        }
    }
}
