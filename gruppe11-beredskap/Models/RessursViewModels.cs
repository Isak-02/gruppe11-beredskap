using System.ComponentModel.DataAnnotations;

namespace gruppe11_beredskap.Models;

public class RessursViewModel
{
    [Required(ErrorMessage = "Du må velge type ressurs")]
    [Display(Name = "Ressurs")]
    public string TypeRessurs { get; set; }

    [Display(Name = "Spesifiser annen type")]
    public string? AnnenType { get; set; }

    [Required(ErrorMessage = "Geografisk område må fylles ut")]
    [Display(Name = "Geografisk område")]
    public string GeografiskOmrade { get; set; }

    [Required]
    [Display(Name = "Tidspunkt/frist")]
    public DateTime Tidspunkt { get; set; }

    [Required]
    [Display(Name = "Kontaktpunkt")]
    public string Kontaktpunkt { get; set; }

    public string Status { get; set; } = "Tilgjengelig";

    // Klargjort for kart integrasjon senere
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}