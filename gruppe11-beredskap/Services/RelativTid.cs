namespace gruppe11_beredskap.Services;

// Kort tekst som «4min siden» eller «4t siden», så tabellen ikke trenger hele datoen.
public static class RelativTid
{
    public static string Tekst(DateTime tidspunkt, DateTime? nå = null)
    {
        var utc = SomUtc(tidspunkt);
        var nåUtc = nå.HasValue ? SomUtc(nå.Value) : DateTime.UtcNow;
        var diff = nåUtc - utc;
        if (diff < TimeSpan.Zero)
            diff = TimeSpan.Zero;

        if (diff.TotalMinutes < 1)
            return "nå";
        if (diff.TotalHours < 1)
            return $"{(int)diff.TotalMinutes}min siden";
        if (diff.TotalDays < 1)
            return $"{(int)diff.TotalHours}t siden";
        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays}d siden";
        if (diff.TotalDays < 30)
            return $"{(int)(diff.TotalDays / 7)}u siden";
        if (diff.TotalDays < 365)
            return $"{(int)(diff.TotalDays / 30)}mnd siden";

        return $"{(int)(diff.TotalDays / 365)}år siden";
    }

    // Hele tidspunktet, til titteltekst når man holder over den korte teksten.
    public static string Absolutt(DateTime tidspunkt)
    {
        return TilLokal(tidspunkt).ToString("dd.MM.yyyy HH:mm");
    }

    private static DateTime SomUtc(DateTime tidspunkt)
    {
        return tidspunkt.Kind switch
        {
            DateTimeKind.Utc => tidspunkt,
            DateTimeKind.Local => tidspunkt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(tidspunkt, DateTimeKind.Utc)
        };
    }

    private static DateTime TilLokal(DateTime tidspunkt)
    {
        var utc = SomUtc(tidspunkt);
        if (TimeZoneInfo.TryFindSystemTimeZoneById("Europe/Oslo", out var oslo))
            return TimeZoneInfo.ConvertTimeFromUtc(utc, oslo);
        return utc.ToLocalTime();
    }
}
