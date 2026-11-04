namespace gruppe11_beredskap.Models.Authorization;

// Navnene på rollene i systemet. Brukes i [Authorize(Roles = ...)] og når brukere får rolle.
public static class RoleNames
{
    // Registrerer og styrer egne behov (offentlige aktører). Opprettes av oss, ikke ved selvregistrering.
    public const string KriseInnmelder = "KriseInnmelder";

    // Registrerer og styrer egne ressurser. Rollen man får når man oppretter bruker selv.
    public const string RessursInnmelder = "RessursInnmelder";

    // Ser alt og kobler behov og ressurser. Opprettes av oss, ikke ved selvregistrering.
    public const string Koordinator = "Koordinator";

    // Hvem som får registrere behov og ressurser. Flere roller skilles med komma i [Authorize].
    public const string KanRegistrereBehov = KriseInnmelder + "," + Koordinator;
    public const string KanRegistrereRessurs = RessursInnmelder + "," + Koordinator;

    public static readonly string[] Alle = { KriseInnmelder, RessursInnmelder, Koordinator };
}
