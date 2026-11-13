using gruppe11_beredskap.Models.Authorization;
using gruppe11_beredskap.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace gruppe11_beredskap.Services;

// Oppretter rollene og testbrukerne når appen starter, hvis de ikke finnes fra før.
// Koordinator og KriseInnmelder kan ikke registrere seg selv, så de lages her.
public static class IdentitySeeder
{
    public const string KoordinatorEpost = "koordinator@kartkrise.local";
    public const string KriseInnmelderEpost = "krise@kartkrise.local";
    public const string RessursInnmelderEpost = "ressurs@kartkrise.local";

    public static async Task SeedRollerAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var rolle in RoleNames.Alle)
        {
            if (await roleManager.RoleExistsAsync(rolle))
                continue;

            var resultat = await roleManager.CreateAsync(new IdentityRole(rolle));
            SjekkResultat(resultat, $"Kunne ikke opprette rollen {rolle}.");
        }
    }

    // Lager én testbruker per rolle. Passordet kommer fra appsettings.Development.json
    // (Testbrukere:Passord), så det ikke ligger i koden.
    public static async Task SeedTestbrukereAsync(UserManager<Bruker> userManager, string passord)
    {
        await LagBrukerAsync(userManager, KoordinatorEpost, "Test Koordinator", "11111111", RoleNames.Koordinator, passord);
        await LagBrukerAsync(userManager, KriseInnmelderEpost, "Test Krise-innmelder", "22222222", RoleNames.KriseInnmelder, passord);
        await LagBrukerAsync(userManager, RessursInnmelderEpost, "Test Ressurs-innmelder", "33333333", RoleNames.RessursInnmelder, passord);
    }

    private static async Task LagBrukerAsync(
        UserManager<Bruker> userManager, string epost, string navn, string telefon, string rolle, string passord)
    {
        var bruker = await userManager.FindByEmailAsync(epost);
        if (bruker == null)
        {
            bruker = new Bruker
            {
                UserName = epost,
                Email = epost,
                EmailConfirmed = true,
                Navn = navn,
                PhoneNumber = telefon
            };

            SjekkResultat(await userManager.CreateAsync(bruker, passord), $"Kunne ikke opprette testbrukeren {epost}.");
        }
        else if (string.IsNullOrWhiteSpace(bruker.PhoneNumber))
        {
            // Testbrukere som ble laget før telefon ble påkrevd, får et nummer nå
            bruker.PhoneNumber = telefon;
            SjekkResultat(await userManager.UpdateAsync(bruker), $"Kunne ikke oppdatere testbrukeren {epost}.");
        }

        if (!await userManager.IsInRoleAsync(bruker, rolle))
        {
            SjekkResultat(await userManager.AddToRoleAsync(bruker, rolle), $"Kunne ikke gi {epost} rollen {rolle}.");
        }
    }

    private static void SjekkResultat(IdentityResult resultat, string feilmelding)
    {
        if (!resultat.Succeeded)
        {
            throw new InvalidOperationException(
                $"{feilmelding} {string.Join(", ", resultat.Errors.Select(e => e.Description))}");
        }
    }
}
