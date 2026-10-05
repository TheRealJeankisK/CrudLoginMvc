using System.ComponentModel.DataAnnotations;

namespace CrudLoginMvc.Models;

// modelo de la tabla Usuarios (cada propiedad es una columna)
public class Usuario
{
    // clave primaria (EF la reconoce por llamarse Id, es autoincrementable)
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(150)]
    public string Correo { get; set; } = "";

    // usuario para el login (unico, se configura en AppDbContext)
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = "";

    // aqui va el hash (nunca la contraseña en texto plano)
    public string PasswordHash { get; set; } = "";

    [Display(Name = "Fecha de creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
