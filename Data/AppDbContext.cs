using CrudLoginMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudLoginMvc.Data;

// contexto de la bd (puente entre las clases de c# y las tablas)
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // tabla Usuarios (cada DbSet es una tabla)
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // nombre de usuario unico
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();
    }
}
