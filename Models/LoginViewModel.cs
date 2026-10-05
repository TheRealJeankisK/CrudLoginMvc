using System.ComponentModel.DataAnnotations;

namespace CrudLoginMvc.Models;

// datos del form de login, no es tabla
public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu usuario.")]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    // la pagina a la que queria entrar antes de que le pida login
    public string? ReturnUrl { get; set; }
}
