using System.Security.Claims;
using CrudLoginMvc.Data;
using CrudLoginMvc.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudLoginMvc.Controllers;

// login y logout
public class CuentaController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Usuario> _hasher;

    public CuentaController(AppDbContext db, IPasswordHasher<Usuario> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // GET /Cuenta/Login muestra el form
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // si viene con ReturnUrl es que quiso entrar a algo protegido sin estar logueado
        if (!string.IsNullOrEmpty(returnUrl))
            ViewBag.Aviso = "No tienes permiso para entrar a esa sección. Inicia sesión primero.";

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST /Cuenta/Login valida usuario y contraseña
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == modelo.NombreUsuario);

        // cifra lo que escribio y lo compara con el hash de la bd
        var passwordCorrecta = usuario != null &&
            _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, modelo.Password)
                != PasswordVerificationResult.Failed;

        if (!passwordCorrecta)
        {
            // mismo mensaje para los dos casos asi no se sabe cual fallo
            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(modelo);
        }

        // claims = datos del usuario que van dentro de la cookie
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario!.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario)
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad));

        // IsLocalUrl para que no te redirijan a otra pagina externa
        if (Url.IsLocalUrl(modelo.ReturnUrl))
            return Redirect(modelo.ReturnUrl!);

        return RedirectToAction("Index", "Usuarios");
    }

    // POST /Cuenta/Logout borra la cookie y regresa al login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
