using CrudLoginMvc.Data;
using CrudLoginMvc.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// servicios de la app
builder.Services.AddControllersWithViews();

// conexion a la bd sqlite (la ruta esta en appsettings.json)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// cifrado de contraseñas (PBKDF2 con salt, mas seguro que md5)
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// aqui configuro el login con cookie
// si no esta logueado y entra a algo protegido lo manda al LoginPath
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.LogoutPath = "/Cuenta/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// aqui creo la bd si no existe y el admin inicial (sino no hay con quien entrar la primera vez)
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

// primero authentication y luego authorization (el orden importa)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
