namespace gruppe11_beredskap.Models;

// Faste statusnavn for behov og ressurser.
public static class BehovStatus
{
    public const string Ny = "Ny";
    public const string Vurderes = "Vurderes";
    public const string Tildelt = "Tildelt";
    public const string Løst = "Løst";
}

public static class RessursStatus
{
    public const string Tilgjengelig = "Tilgjengelig";
    public const string Tildelt = "Tildelt";
}
