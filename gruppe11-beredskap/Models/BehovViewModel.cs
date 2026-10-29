using System.ComponentModel.DataAnnotations;

namespace gruppe11_beredskap.Models;

//Registrer behov side og eventuelle feilmeldinger
public class BehovViewModel

{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Du må velge type behov")]
    [Display(Name = "Type behov")]
    public string TypeBehov { get; set; }

    [Display(Name = "Geografisk område")]
    public string? GeografiskOmrade { get; set; }

    [Required]
    [Display(Name = "Tidspunkt/frist")]
    public DateTime Tidspunkt { get; set; }

    [Required]
    [Display(Name = "Prioritet")]
    public string Prioritet { get; set; }

    [Required]
    [Display(Name = "Kontaktpunkt")]
    public string Kontaktpunkt { get; set; }

    public string Status { get; set; } = "Ny";

    [Required(ErrorMessage = "Du må tegne en markør eller sirkel på kartet for å velge sted")]
    public double? Latitude { get; set; }

    [Required(ErrorMessage = "Du må tegne en markør eller sirkel på kartet for å velge sted")]
    public double? Longitude { get; set; }

    // Valgfri. Settes bare når brukeren tegner en sirkel på kartet.
    [Range(1, double.MaxValue, ErrorMessage = "Radius må være større enn 0 meter")]
    [Display(Name = "Radius (meter)")]
    public double? Radius { get; set; }
}