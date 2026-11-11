using gruppe11_beredskap.Services;

namespace gruppe11_beredskap.Tests;

public class RelativTidTests
{
    private static readonly DateTime Nå = new(2026, 11, 11, 16, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void UnderEttMinutt_ViserNå()
    {
        var tekst = RelativTid.Tekst(Nå.AddSeconds(-30), Nå);

        Assert.Equal("nå", tekst);
    }

    [Fact]
    public void FireMinutter_ViserMinutter()
    {
        var tekst = RelativTid.Tekst(Nå.AddMinutes(-4), Nå);

        Assert.Equal("4min siden", tekst);
    }

    [Fact]
    public void FireTimer_ViserTimer()
    {
        var tekst = RelativTid.Tekst(Nå.AddHours(-4), Nå);

        Assert.Equal("4t siden", tekst);
    }

    [Fact]
    public void ToDager_ViserDager()
    {
        var tekst = RelativTid.Tekst(Nå.AddDays(-2), Nå);

        Assert.Equal("2d siden", tekst);
    }

    [Fact]
    public void Absolutt_ViserNorskKlokkeslett()
    {
        var tekst = RelativTid.Absolutt(new DateTime(2026, 11, 11, 16, 38, 0, DateTimeKind.Utc));

        Assert.Equal("11.11.2026 17:38", tekst);
    }
}