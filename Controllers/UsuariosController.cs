using System.Security.Claims;
using CrudLoginMvc.Data;
using CrudLoginMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudLoginMvc.Controllers;

// [Authorize] protege TODAS las acciones de este controlador:
// sin sesión iniciada, cualquier URL /Usuarios/... redirige al login.
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

    // READ (lista) — GET /Usuarios
    public async Task<IActionResult> Index()
    {
        var usuarios = await _db.Usuarios.OrderBy(u => u.Nombre).ToListAsync();
        return View(usuarios);
    }

    // READ (detalle) — GET /Usuarios/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();
        return View(usuario);
    }

    // CREATE — GET /Usuarios/Create (muestra el formulario vacío)
    public IActionResult Create()
    {
        return View(new UsuarioFormViewModel());
    }

    // CREATE — POST /Usuarios/Create (guarda)
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

    // UPDATE — GET /Usuarios/Edit/5 (formulario con los datos actuales)
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

    // UPDATE — POST /Usuarios/Edit/5 (guarda los cambios)
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

        // Solo se cambia la contraseña si se escribió una nueva.
        if (!string.IsNullOrWhiteSpace(form.Password))
            usuario.PasswordHash = _hasher.HashPassword(usuario, form.Password);

        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"Usuario \"{usuario.NombreUsuario}\" actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // DELETE — GET /Usuarios/Delete/5 (pide confirmación)
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();
        return View(usuario);
    }

    // DELETE — POST /Usuarios/Delete/5 (borra de verdad)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        // No se permite borrar la cuenta con la que se inició sesión.
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

    // --- Métodos de apoyo (DRY: se usan en varias acciones) ---

    private Task<bool> NombreUsuarioOcupado(string nombreUsuario, int idActual) =>
        _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario && u.Id != idActual);

    private int IdUsuarioActual() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
