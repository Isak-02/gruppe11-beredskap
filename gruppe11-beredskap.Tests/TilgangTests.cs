using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using gruppe11_beredskap.Controllers;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Authorization;

namespace gruppe11_beredskap.Tests;

// Sjekker at sidene har riktig [Authorize(Roles = ...)], altså at bare riktige roller får tilgang.
// Testene leser attributtene på controllerne (refleksjon), siden selve tilgangssjekken skjer i ASP.NET.
public class TilgangTests
{
    private static string? RollerPåMetode(Type controller, string metode, params Type[] parametere)
    {
        var metodeInfo = controller.GetMethod(metode, parametere)
            ?? throw new InvalidOperationException($"Fant ikke {controller.Name}.{metode}");
        return metodeInfo.GetCustomAttribute<AuthorizeAttribute>()?.Roles;
    }

    [Fact]
    public void KanRegistrereBehov_GjelderKriseInnmelderOgKoordinator()
    {
        var roller = RoleNames.KanRegistrereBehov.Split(',');

        Assert.Contains(RoleNames.KriseInnmelder, roller);
        Assert.Contains(RoleNames.Koordinator, roller);
        Assert.DoesNotContain(RoleNames.RessursInnmelder, roller);
    }

    [Fact]
    public void KanRegistrereRessurs_GjelderRessursInnmelderOgKoordinator()
    {
        var roller = RoleNames.KanRegistrereRessurs.Split(',');

        Assert.Contains(RoleNames.RessursInnmelder, roller);
        Assert.Contains(RoleNames.Koordinator, roller);
        Assert.DoesNotContain(RoleNames.KriseInnmelder, roller);
    }

    [Fact]
    public void Behov_RegistrerOgMine_KreverKanRegistrereBehov()
    {
        Assert.Equal(RoleNames.KanRegistrereBehov, RollerPåMetode(typeof(BehovController), "Registrer"));
        Assert.Equal(RoleNames.KanRegistrereBehov, RollerPåMetode(typeof(BehovController), "Registrer", typeof(BehovViewModel)));
        Assert.Equal(RoleNames.KanRegistrereBehov, RollerPåMetode(typeof(BehovController), "Mine"));
    }

    [Fact]
    public void Ressurs_RegistrerOgMine_KreverKanRegistrereRessurs()
    {
        Assert.Equal(RoleNames.KanRegistrereRessurs, RollerPåMetode(typeof(RessursController), "Registrer"));
        Assert.Equal(RoleNames.KanRegistrereRessurs, RollerPåMetode(typeof(RessursController), "Registrer", typeof(RessursViewModel)));
        Assert.Equal(RoleNames.KanRegistrereRessurs, RollerPåMetode(typeof(RessursController), "Mine"));
    }

    [Fact]
    public void AlleBehovOgAlleRessurser_KreverKoordinator()
    {
        Assert.Equal(RoleNames.Koordinator, RollerPåMetode(typeof(BehovController), "Oversikt"));
        Assert.Equal(RoleNames.Koordinator, RollerPåMetode(typeof(RessursController), "Oversikt"));
    }

    [Fact]
    public void RessurserPerFylke_KreverKoordinator()
    {
        Assert.Equal(RoleNames.Koordinator, RollerPåMetode(typeof(RessursController), "PerFylke"));
    }

    [Fact]
    public void Kobling_KreverKoordinator()
    {
        var authorize = typeof(MatchingController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal(RoleNames.Koordinator, authorize.Roles);
    }

    [Fact]
    public void Innlogging_OgRegistrering_ErÅpneForAlle()
    {
        var login = typeof(AccountController).GetMethod("Login", new[] { typeof(LoggInnViewModel), typeof(string) });
        var register = typeof(AccountController).GetMethod("Register", new[] { typeof(RegistrerBrukerViewModel), typeof(string) });

        Assert.NotNull(login!.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(register!.GetCustomAttribute<AllowAnonymousAttribute>());
    }
}
