﻿using BlogMVC.Data;
using BlogMVC.Entity;
using BlogMVC.Models;
using BlogMVC.Services;
using BlogMVC.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogMVC.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicioUsuarios _servicesUsuario;

        public CommentsController(ApplicationDbContext context,
            IServicioUsuarios servicesUsuario)
        {
            _context = context;
            _servicesUsuario = servicesUsuario;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Comentar(EntrysCommentVM model)
        {
            if (!ModelState.IsValid) 
            {
                return RedirectToAction("Details", "Entradas", new { id = model.Id });
            }

            var existeEntry = await _context.Entries.AnyAsync(c => c.Id == model.Id);

            if (!existeEntry)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var usuarioId = _servicesUsuario.ObtenerUsuarioId()!;

            var comentario = new Comment()
            {
                EntradaId = model.Id,
                Cuerpo = model.Cuerpo,
                UsuarioId = usuarioId,
                FechaPublicacion = DateTime.UtcNow
            };

            _context.Add(comentario);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Entradas", new { id = model.Id });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Borrar(int id)
        {
            var comentario = await _context.Comments.FindAsync(id);

            if (comentario is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var usuarioId = _servicesUsuario.ObtenerUsuarioId();

            var puedeBorrar = await _servicesUsuario.PuedeUsuarioHacerCrudComentarios();

            if (usuarioId != comentario.UsuarioId && !puedeBorrar)
            {
                var urlRetorno = HttpContext.ObtenerUrlRetorno();
                return RedirectToAction("Login", "Users", new { urlRetorno });
            }

            return View(comentario);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BorrarComentario(int id)
        {
            var comentario = await _context.Comments.FindAsync(id);

            if (comentario is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var usuarioId = _servicesUsuario.ObtenerUsuarioId();

            var puedeBorrar = await _servicesUsuario.PuedeUsuarioHacerCrudComentarios();

            if (usuarioId != comentario.UsuarioId && !puedeBorrar)
            {
                var urlRetorno = HttpContext.ObtenerUrlRetorno();
                return RedirectToAction("Login", "Users", new { urlRetorno });
            }

            comentario.Borrado = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Entradas", new { id = comentario.Id });
        }
    }
}
