using System.ComponentModel.DataAnnotations;

namespace gruppe11_beredskap.Models;

//Registrer behov side og eventuelle feilmeldinger
public class BehovViewModel

{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Du må velge type behov")]
    [Display(Name = "Type behov")]
    public string TypeBehov { get; set; }

    [Required(ErrorMessage = "Du må skrive en beskrivelse av behovet")]
    [StringLength(1000, ErrorMessage = "Beskrivelsen kan være maks 1000 tegn")]
    [Display(Name = "Beskrivelse")]
    public string Beskrivelse { get; set; }

    [Display(Name = "Geografisk område")]
    public string? GeografiskOmrade { get; set; }

    [Required(ErrorMessage = "Du må velge tidspunkt/frist")]
    [Display(Name = "Tidspunkt/frist")]
    public DateTime Tidspunkt { get; set; }

    [Required(ErrorMessage = "Du må velge prioritet")]
    [Display(Name = "Prioritet")]
    public string Prioritet { get; set; }

    [Required(ErrorMessage = "Du må skrive inn et kontaktpunkt")]
    [Display(Name = "Kontaktpunkt")]
    public string Kontaktpunkt { get; set; }

    public string Status { get; set; } = "Ny";

    [Required(ErrorMessage = "Du må tegne en markør eller sirkel på kartet for å velge sted")]
    public double? Latitude { get; set; }

    [Required(ErrorMessage = "Du må tegne en markør eller sirkel på kartet for å velge sted")]
    public double? Longitude { get; set; }

    // Valgfri. Settes bare når brukeren tegner en sirkel på kartet.
    // Maks 50 km, så ingen kan tegne en sirkel over store deler av landet
    [Range(1.0, 50000.0, ErrorMessage = "Radius må være mellom 1 og 50 000 meter")]
    [Display(Name = "Radius (meter)")]
    public double? Radius { get; set; }
}