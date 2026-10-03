using System.ComponentModel.DataAnnotations;

namespace CrudLoginMvc.Models;

// ViewModel: solo los datos que necesita el formulario de login (no es una tabla).
public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu usuario.")]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    // Página a la que se quería entrar antes de que pidiera iniciar sesión.
    public string? ReturnUrl { get; set; }
}
