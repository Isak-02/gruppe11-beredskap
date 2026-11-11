using System.ComponentModel.DataAnnotations;

namespace gruppe11_beredskap.Models;

//melde inn ressurs side og eventeulle feilmeldinger b
public class RessursViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Du må velge type ressurs")]
    [Display(Name = "Type ressurs")]
    public string TypeRessurs { get; set; }

    [Required(ErrorMessage = "Du må skrive en beskrivelse av ressursen")]
    [StringLength(1000, ErrorMessage = "Beskrivelsen kan være maks 1000 tegn")]
    [Display(Name = "Beskrivelse")]
    public string Beskrivelse { get; set; }

    [StringLength(200, ErrorMessage = "Typen kan være maks 200 tegn")]
    [Display(Name = "Spesifiser annen type")]
    public string? AnnenType { get; set; }

    [Display(Name = "Drivstofftype")] public string? Drivstofftype { get; set; }

    // Øvre grense så ingen kan skrive inn urealistisk store tall. 60 tonn er omtrent et fullt vogntog.
    [Range(0.01, 60.0, ErrorMessage = "Lastekapasiteten må være mellom 0,01 og 60 tonn")]
    [Display(Name = "Lastekapasitet (tonn)")]
    public decimal? Lastekapasitet { get; set; }

    [Display(Name = "Geografisk område")] public string? GeografiskOmrade { get; set; }

    // Settes automatisk når ressursen lagres. Vises i tabellen, fylles ikke inn i skjemaet.
    [Display(Name = "Opprettet")]
    public DateTime OpprettetTidspunkt { get; set; }

    [Required(ErrorMessage = "Du må skrive inn et kontaktpunkt")]
    [Display(Name = "Kontaktpunkt")]
    public string Kontaktpunkt { get; set; }

    public string Status { get; set; } = "Tilgjengelig";

    [Required(ErrorMessage = "Du må sette en markør på kartet for å velge sted")]
    public double? Latitude { get; set; }

    [Required(ErrorMessage = "Du må sette en markør på kartet for å velge sted")]
    public double? Longitude { get; set; }
}