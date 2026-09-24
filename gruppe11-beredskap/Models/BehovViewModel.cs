using System.ComponentModel.DataAnnotations;

namespace gruppe11_beredskap.Models;

//Krav og eventuelle feilmeldinge

public class BehovViewModel
{
    [Required(ErrorMessage = "Du må velge type behov")]
    [Display(Name = "Behov")]
    public string TypeBehov { get; set; }

    [Required(ErrorMessage = "Geografisk område må markeres")]
    [Display(Name = "Geografisk område")]
    public string GeografiskOmrade { get; set; }

    [Required]
    [Display(Name = "Tidspunkt/frist")]
    public DateTime Tidspunkt { get; set; }

    [Required]
    [Display(Name = "Prioritet")]
    public string Prioritet { get; set; }

    //fjernes senere fordi all info skal komme automatisk fra innlogging.
    [Required]
    [Display(Name = "Kontaktpunkt")]
    public string Kontaktpunkt { get; set; }

    public string Status { get; set; } = "Ny";

    // Klargjort for kart-integrasjon til filip
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}