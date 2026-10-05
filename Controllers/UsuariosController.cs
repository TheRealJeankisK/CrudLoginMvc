using System.Security.Claims;
using CrudLoginMvc.Data;
using CrudLoginMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudLoginMvc.Controllers;

// CRUD de usuarios
// [Authorize] protege todo el controller (sin login cualquier url /Usuarios/... manda al login)
[Authorize]
public class UsuariosController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Usuario> _hasher;

    public UsuariosController(AppDbContext db, IPasswordHasher<Usuario> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // listar usuarios (GET /Usuarios)
    public async Task<IActionResult> Index()
    {
        var usuarios = await _db.Usuarios.OrderBy(u => u.Nombre).ToListAsync();
        return View(usuarios);
    }

    // ver detalle (GET /Usuarios/Details/5)
    public async Task<IActionResult> Details(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();
        return View(usuario);
    }

    // mostrar form vacio para crear (GET)
    public IActionResult Create()
    {
        return View(new UsuarioFormViewModel());
    }

    // crear usuario (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioFormViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.Password))
            ModelState.AddModelError(nameof(form.Password), "La contraseña es obligatoria.");

        if (await NombreUsuarioOcupado(form.NombreUsuario, idActual: 0))
            ModelState.AddModelError(nameof(form.NombreUsuario), "Ese usuario ya existe.");

        if (!ModelState.IsValid)
            return View(form);

        var usuario = new Usuario
        {
            Nombre = form.Nombre,
            Correo = form.Correo,
            NombreUsuario = form.NombreUsuario
        };
        usuario.PasswordHash = _hasher.HashPassword(usuario, form.Password!);

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"Usuario \"{usuario.NombreUsuario}\" creado.";
        return RedirectToAction(nameof(Index));
    }

    // mostrar form con los datos actuales (GET)
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        var form = new UsuarioFormViewModel
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            NombreUsuario = usuario.NombreUsuario
        };
        return View(form);
    }

    // editar usuario (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UsuarioFormViewModel form)
    {
        if (id != form.Id) return BadRequest();

        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        if (await NombreUsuarioOcupado(form.NombreUsuario, idActual: id))
            ModelState.AddModelError(nameof(form.NombreUsuario), "Ese usuario ya existe.");

        if (!ModelState.IsValid)
            return View(form);

        usuario.Nombre = form.Nombre;
        usuario.Correo = form.Correo;
        usuario.NombreUsuario = form.NombreUsuario;

        // aqui solo cambio la contraseña si escribio una nueva
        if (!string.IsNullOrWhiteSpace(form.Password))
            usuario.PasswordHash = _hasher.HashPassword(usuario, form.Password);

        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"Usuario \"{usuario.NombreUsuario}\" actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // pedir confirmacion para eliminar (GET)
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();
        return View(usuario);
    }

    // eliminar usuario (POST)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        // no dejar que se borre a si mismo
        if (usuario.Id == IdUsuarioActual())
        {
            TempData["Error"] = "No puedes eliminar tu propia cuenta mientras la usas.";
            return RedirectToAction(nameof(Index));
        }

        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"Usuario \"{usuario.NombreUsuario}\" eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // metodos de apoyo (se usan en varias acciones, DRY)

    private Task<bool> NombreUsuarioOcupado(string nombreUsuario, int idActual) =>
        _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario && u.Id != idActual);

    private int IdUsuarioActual() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
