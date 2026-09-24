using System.ComponentModel.DataAnnotations;

namespace gruppe11_beredskap.Models;

//melde inn ressurs side og eventeulle feilmeldinger b
public class RessursViewModel
{
    [Required(ErrorMessage = "Du må velge type ressurs")]
    [Display(Name = "Type ressurs")]
    public string TypeRessurs { get; set; }

    [Display(Name = "Spesifiser annen type")]
    public string? AnnenType { get; set; }

    [Display(Name = "Geografisk område")]
    public string? GeografiskOmrade { get; set; }

    [Required]
    [Display(Name = "Tidspunkt/frist")]
    public DateTime Tidspunkt { get; set; }

    [Required]
    [Display(Name = "Kontaktpunkt")]
    public string Kontaktpunkt { get; set; }

    public string Status { get; set; } = "Tilgjengelig";

    [Required(ErrorMessage = "Du må klikke på kartet for å velge sted")]
    public double? Latitude { get; set; }

    [Required(ErrorMessage = "Du må klikke på kartet for å velge sted")]
    public double? Longitude { get; set; }
}