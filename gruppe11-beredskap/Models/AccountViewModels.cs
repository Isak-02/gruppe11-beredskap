using System.ComponentModel.DataAnnotations;

namespace gruppe11_beredskap.Models;

// Skjemaet "Logg inn"
public class LoggInnViewModel
{
    [Required(ErrorMessage = "Du må skrive inn e-postadressen din")]
    [EmailAddress(ErrorMessage = "E-postadressen er ikke gyldig")]
    [Display(Name = "E-postadresse")]
    public string Epost { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du må skrive inn passordet ditt")]
    [DataType(DataType.Password)]
    [Display(Name = "Passord")]
    public string Passord { get; set; } = string.Empty;
}

// Skjemaet "Opprett bruker". Den som oppretter bruker selv, blir alltid ressurs-innmelder.
public class RegistrerBrukerViewModel
{
    [Required(ErrorMessage = "Du må skrive inn navnet ditt")]
    [StringLength(200, ErrorMessage = "Navnet kan være maks 200 tegn")]
    [Display(Name = "Navn")]
    public string Navn { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du må skrive inn e-postadressen din")]
    [EmailAddress(ErrorMessage = "E-postadressen er ikke gyldig")]
    [Display(Name = "E-postadresse")]
    public string Epost { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Telefonnummeret er ikke gyldig")]
    [Display(Name = "Telefonnummer")]
    public string? Telefon { get; set; }

    [Required(ErrorMessage = "Du må velge et passord")]
    [DataType(DataType.Password)]
    [Display(Name = "Passord")]
    public string Passord { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du må skrive passordet en gang til")]
    [DataType(DataType.Password)]
    [Compare(nameof(Passord), ErrorMessage = "Passordene er ikke like")]
    [Display(Name = "Bekreft passord")]
    public string BekreftPassord { get; set; } = string.Empty;
}

// Kortet med "Logg inn" og "Opprett bruker", som brukes på forsiden og på /Account/Login og /Account/Register.
public class InnloggingViewModel
{
    public LoggInnViewModel LoggInn { get; set; } = new();
    public RegistrerBrukerViewModel Registrer { get; set; } = new();

    // true: vis "Opprett bruker" i stedet for "Logg inn" når siden åpnes
    public bool VisRegistrering { get; set; }

    // Siden brukeren skal sendes til etter innlogging, for eksempel /Ressurs/Registrer
    public string? ReturnUrl { get; set; }

    // Feil som ikke hører til ett bestemt felt, for eksempel feil passord
    public string? LoggInnFeil { get; set; }
    public List<string> RegistrerFeil { get; set; } = new();
}

// Skjemaet "Glemt passord"
public class GlemtPassordViewModel
{
    [Required(ErrorMessage = "Du må skrive inn e-postadressen din")]
    [EmailAddress(ErrorMessage = "E-postadressen er ikke gyldig")]
    [Display(Name = "E-postadresse")]
    public string Epost { get; set; } = string.Empty;
}

// Skjemaet "Nytt passord", som åpnes fra lenken i e-posten
public class TilbakestillPassordViewModel
{
    [Required]
    [EmailAddress]
    public string Epost { get; set; } = string.Empty;

    [Required]
    public string Kode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du må velge et nytt passord")]
    [DataType(DataType.Password)]
    [Display(Name = "Nytt passord")]
    public string Passord { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du må skrive passordet en gang til")]
    [DataType(DataType.Password)]
    [Compare(nameof(Passord), ErrorMessage = "Passordene er ikke like")]
    [Display(Name = "Bekreft nytt passord")]
    public string BekreftPassord { get; set; } = string.Empty;
}
