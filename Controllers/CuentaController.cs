using System.Security.Claims;
using CrudLoginMvc.Data;
using CrudLoginMvc.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudLoginMvc.Controllers;

// Controlador de la cuenta: iniciar y cerrar sesión.
public class CuentaController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Usuario> _hasher;

    public CuentaController(AppDbContext db, IPasswordHasher<Usuario> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // GET /Cuenta/Login → muestra el formulario.
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Si llega con ReturnUrl es porque intentó entrar a una página protegida sin sesión.
        if (!string.IsNullOrEmpty(returnUrl))
            ViewBag.Aviso = "No tienes permiso para entrar a esa sección. Inicia sesión primero.";

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST /Cuenta/Login → revisa usuario y contraseña.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == modelo.NombreUsuario);

        // Se cifra la contraseña escrita y se compara con el hash guardado.
        var passwordCorrecta = usuario != null &&
            _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, modelo.Password)
                != PasswordVerificationResult.Failed;

        if (!passwordCorrecta)
        {
            // Mismo mensaje para usuario o contraseña: así no se revela cuál de los dos falló.
            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(modelo);
        }

        // "Claims": los datos del usuario que viajan dentro de la cookie.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario!.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario)
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad));

        // Url.IsLocalUrl evita redirigir a sitios externos (ataque de "open redirect").
        if (Url.IsLocalUrl(modelo.ReturnUrl))
            return Redirect(modelo.ReturnUrl!);

        return RedirectToAction("Index", "Usuarios");
    }

    // POST /Cuenta/Logout → borra la cookie y vuelve al login.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
