using CrudLoginMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudLoginMvc.Data;

// El DbContext es el puente entre las clases de C# y la base de datos.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Cada DbSet es una tabla. Con db.Usuarios se consulta, agrega o borra.
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // No pueden existir dos usuarios con el mismo nombre de usuario.
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();
    }
}
