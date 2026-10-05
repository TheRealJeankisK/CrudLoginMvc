using System.ComponentModel.DataAnnotations;

namespace CrudLoginMvc.Models;

// form para crear y editar usuarios
// uso esto y no Usuario porque aqui llega la contraseña en texto para cifrarla, la tabla solo tiene el hash
public class UsuarioFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(150)]
    public string Correo { get; set; } = "";

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = "";

    // al crear es obligatoria (lo valido en el controller), al editar si va vacia no se cambia
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [Display(Name = "Contraseña")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string? ConfirmarPassword { get; set; }
}
