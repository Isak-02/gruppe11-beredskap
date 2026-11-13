using System.Security.Claims;
using gruppe11_beredskap.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace gruppe11_beredskap.Services;

// Kontaktinformasjonen til den innloggede brukeren. Brukes som kontaktpunkt
// når brukeren registrerer et behov eller en ressurs, så hen slipper å skrive det inn selv.
public record Kontaktinfo(string Navn, string? Telefon, string? Epost)
{
    // Teksten som lagres i Kontaktpunkt-kolonnen, f.eks. "Ola Nordmann, 12345678, ola@eksempel.no".
    // Felt som mangler (telefon er valgfritt ved registrering) hoppes over.
    public string SomTekst()
    {
        var deler = new[] { Navn, Telefon, Epost }.Where(d => !string.IsNullOrWhiteSpace(d));
        return string.Join(", ", deler);
    }
}

public interface IKontaktinfoTjeneste
{
    // Er null hvis ingen er logget inn eller brukeren ikke finnes
    Task<Kontaktinfo?> HentAsync(ClaimsPrincipal bruker);
}

// Henter navn, telefon og e-post fra brukertabellen (Identity)
public class KontaktinfoTjeneste : IKontaktinfoTjeneste
{
    private readonly UserManager<Bruker> _userManager;

    public KontaktinfoTjeneste(UserManager<Bruker> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Kontaktinfo?> HentAsync(ClaimsPrincipal bruker)
    {
        var funnet = await _userManager.GetUserAsync(bruker);
        if (funnet == null)
            return null;

        var navn = string.IsNullOrWhiteSpace(funnet.Navn) ? funnet.Email ?? "" : funnet.Navn;
        return new Kontaktinfo(navn, funnet.PhoneNumber, funnet.Email);
    }
}
