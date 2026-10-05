using CrudLoginMvc.Data;
using CrudLoginMvc.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// servicios de la app
builder.Services.AddControllersWithViews();

// bd en sqlite, es un solo archivo crudlogin.db (la ruta esta en appsettings.json)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// para cifrar las contraseñas, usa PBKDF2 con salt que es mas seguro que md5
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// login con cookie, cuando inicias sesion el navegador guarda la cookie
// si no estas logueado y entras a algo protegido te manda al LoginPath
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.LogoutPath = "/Cuenta/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// al arrancar crea la bd si no existe y mete el admin inicial
// sino no habria con quien entrar la primera vez
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
    db.Database.EnsureCreated();

    if (!db.Usuarios.Any())
    {
        var datos = builder.Configuration.GetSection("AdminInicial");
        var admin = new Usuario
        {
            Nombre = datos["Nombre"]!,
            Correo = datos["Correo"]!,
            NombreUsuario = datos["NombreUsuario"]!
        };
        admin.PasswordHash = hasher.HashPassword(admin, datos["Password"]!);
        db.Usuarios.Add(admin);
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ojo el orden importa, primero ve quien eres (authentication) y despues que puedes hacer (authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
