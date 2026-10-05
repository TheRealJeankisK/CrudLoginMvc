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

    // mostrar form de login (GET)
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // aviso si quiso entrar a algo protegido sin login (viene con ReturnUrl)
        if (!string.IsNullOrEmpty(returnUrl))
            ViewBag.Aviso = "No tienes permiso para entrar a esa sección. Inicia sesión primero.";

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // validar usuario y contraseña (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == modelo.NombreUsuario);

        // aqui cifro lo que escribio y lo comparo con el hash de la bd
        var passwordCorrecta = usuario != null &&
            _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, modelo.Password)
                != PasswordVerificationResult.Failed;

        if (!passwordCorrecta)
        {
            // mismo mensaje para los dos casos (asi no se sabe cual fallo)
            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(modelo);
        }

        // datos del usuario que van en la cookie (claims)
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario!.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario)
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad));

        // regresar a la pagina que queria (IsLocalUrl evita que redirija a una pagina externa)
        if (Url.IsLocalUrl(modelo.ReturnUrl))
            return Redirect(modelo.ReturnUrl!);

        return RedirectToAction("Index", "Usuarios");
    }

    // cerrar sesion, borra la cookie (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
