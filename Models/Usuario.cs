using System.ComponentModel.DataAnnotations;

namespace CrudLoginMvc.Models;

// Modelo: representa la tabla "Usuarios" de la base de datos.
// Cada propiedad es una columna.
public class Usuario
{
    // Clave primaria. EF Core la reconoce por llamarse "Id" y la autoincrementa.
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(150)]
    public string Correo { get; set; } = "";

    // Con este nombre se inicia sesión. Es único (ver AppDbContext).
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = "";

    // Nunca se guarda la contraseña en texto plano: solo su hash (cifrado de una vía).
    public string PasswordHash { get; set; } = "";

    [Display(Name = "Fecha de creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
