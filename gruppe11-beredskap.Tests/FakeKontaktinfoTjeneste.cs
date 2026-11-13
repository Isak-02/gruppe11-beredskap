using System.Security.Claims;
using gruppe11_beredskap.Services;

namespace gruppe11_beredskap.Tests;

// Falsk kontaktinfo, så testene ikke trenger UserManager og database.
// Sett Kontakt til null for å late som om brukeren ikke finnes.
public class FakeKontaktinfoTjeneste : IKontaktinfoTjeneste
{
    public Kontaktinfo? Kontakt { get; set; } = new("Testperson", "12345678", "test@eksempel.no");

    public Task<Kontaktinfo?> HentAsync(ClaimsPrincipal bruker)
    {
        return Task.FromResult(Kontakt);
    }
}
