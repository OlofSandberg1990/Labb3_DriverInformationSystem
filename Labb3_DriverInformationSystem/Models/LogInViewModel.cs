using System.ComponentModel.DataAnnotations;

public class LogInViewModel
{
    [Required(ErrorMessage = "E-post är obligatoriskt")]
    [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Lösenord är obligatoriskt")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
