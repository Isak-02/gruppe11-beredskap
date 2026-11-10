namespace gruppe11_beredskap.Services;

// Enkel kobling mellom type behov og type ressurs.
// Trenger ikke treffe perfekt: transport kan for eksempel dekkes av en lastebil.
// Samme regler brukes i skriptet på koblingssiden (Views/Matching/Index.cshtml).
public static class BehovRessursTreff
{
    public static IReadOnlyDictionary<string, string[]> Kart { get; } =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Transport"] = ["Lastebil", "Henger", "Traktor", "ATV", "Båt", "Snøskuter"],
            ["Droneobservasjon"] = ["Drone og droneoperatør"],
            ["Strøm/aggregat"] = ["Aggregat"],
            ["Snørydding"] = ["Traktor", "Hjullaster", "Lastebil", "Snøskuter"],
            ["Sand/grus"] = ["Sand/grus", "Lastebil", "Henger", "Hjullaster"],
            ["Maskiner"] = ["Gravemaskin", "Hjullaster", "Skogsmaskin", "Traktor"],
            ["Evakuering"] = ["Båt", "Lastebil", "ATV", "Mannskap/frivillige"],
            ["Samband"] = ["Sambandsutstyr"],
            ["Mannskap"] = ["Mannskap/frivillige"],
            ["Lokaler"] = ["Lokaler"]
        };

    public static bool Matcher(string? typeBehov, string? typeRessurs, string? annenType, string? ressursBeskrivelse)
    {
        if (string.IsNullOrWhiteSpace(typeBehov))
            return false;

        var navn = typeRessurs == "Annet" && !string.IsNullOrWhiteSpace(annenType)
            ? annenType
            : typeRessurs;

        if (Kart.TryGetValue(typeBehov.Trim(), out var mulige) &&
            mulige.Any(mulig => Treffer(navn, mulig) || Treffer(ressursBeskrivelse, mulig)))
            return true;

        return Treffer(navn, typeBehov) || Treffer(ressursBeskrivelse, typeBehov);
    }

    // «lastebil med kran» treffer «Lastebil». Et kort ord som «bil» treffer ikke «Lastebil».
    private static bool Treffer(string? tekst, string mulig)
    {
        if (string.IsNullOrWhiteSpace(tekst) || string.IsNullOrWhiteSpace(mulig))
            return false;

        if (tekst.Contains(mulig, StringComparison.OrdinalIgnoreCase))
            return true;

        var kort = tekst.Trim();
        return kort.Length >= 4 && mulig.Contains(kort, StringComparison.OrdinalIgnoreCase);
    }
}
