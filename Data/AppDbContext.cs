using CrudLoginMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudLoginMvc.Data;

// puente entre las clases de c# y la bd
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // cada DbSet es una tabla, con db.Usuarios consultas, agregas o borras
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // que no se repita el nombre de usuario
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();
    }
}
