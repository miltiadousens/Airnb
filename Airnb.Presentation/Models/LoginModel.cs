using System.ComponentModel.DataAnnotations;
namespace Airnb.Presentation.Models;

public class LoginModel
{
    [Required(ErrorMessage = "Skriv din e-mail.")]
    [EmailAddress(ErrorMessage = "Skriv en gyldig e-mailadresse.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Skriv en gyldig adgangskode.")]
    public string Password { get; set; } = "";
}