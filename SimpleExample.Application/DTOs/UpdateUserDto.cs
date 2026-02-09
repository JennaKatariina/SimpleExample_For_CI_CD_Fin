using System.ComponentModel.DataAnnotations;

namespace SimpleExample.Application.DTOs;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Etunimi on pakollinen")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Etunimen tulee olla 3-100 merkkiä")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sukunimi on pakollinen")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Sukunimen tulee olla 3-100 merkkiä")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sähköposti on pakollinen")]
    [EmailAddress(ErrorMessage = "Sähköpostin tulee olla kelvollinen")]
    [StringLength(255, ErrorMessage = "Sähköposti voi olla enintään 255 merkkiä")]
    public string Email { get; set; } = string.Empty;
}
