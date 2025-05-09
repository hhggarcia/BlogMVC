﻿using BlogMVC.Data;
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

                return RedirectToAction("Details", new { id = entry.Id });
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = $"{Constantes.RolAdmin}, {Constantes.CRUDEntradas}")]
        public async Task<IActionResult> Editar(int id)
        {
            var entrada = await _context.Entries.FirstOrDefaultAsync(c => c.Id == id);

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
        public async Task<IActionResult> Editar(EntryEditVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entradaDb = await _context.Entries.FirstOrDefaultAsync(x => x.Id == model.Id);

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
    }
}
