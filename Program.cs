using CrudLoginMvc.Data;
using CrudLoginMvc.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Base de datos: SQLite, un solo archivo (crudlogin.db). La ruta está en appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicio que cifra y verifica contraseñas (PBKDF2 con "salt"; más seguro que MD5).
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Login con cookie: al iniciar sesión el navegador guarda una cookie cifrada que identifica al usuario.
// Si alguien sin sesión entra a una página protegida, se le redirige a LoginPath.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.LogoutPath = "/Cuenta/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Al arrancar: crea la base de datos si no existe y agrega el administrador inicial,
// para poder iniciar sesión la primera vez.
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// El orden importa: primero se identifica quién es (autenticación), luego qué puede hacer (autorización).
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
