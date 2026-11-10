using gruppe11_beredskap.Services;

namespace gruppe11_beredskap.Tests;

public class BehovRessursTreffTests
{
    [Fact]
    public void Transport_TrefferLastebil()
    {
        Assert.True(BehovRessursTreff.Matcher("Transport", "Lastebil", null, null));
    }

    [Fact]
    public void Transport_TrefferIkkeLokaler()
    {
        Assert.False(BehovRessursTreff.Matcher("Transport", "Lokaler", null, "Møterom"));
    }

    [Fact]
    public void Droneobservasjon_TrefferDrone()
    {
        Assert.True(BehovRessursTreff.Matcher("Droneobservasjon", "Drone og droneoperatør", null, null));
    }

    [Fact]
    public void Annet_MedLastebilINavnet_TrefferTransport()
    {
        Assert.True(BehovRessursTreff.Matcher("Transport", "Annet", "Lastebil med kran", null));
    }

    [Fact]
    public void Beskrivelse_MedLastebil_TrefferTransport()
    {
        Assert.True(BehovRessursTreff.Matcher("transport", "Traktor", null, "Kan også kjøre lastebil"));
    }

    [Fact]
    public void SandGrus_TrefferSandGrus()
    {
        Assert.True(BehovRessursTreff.Matcher("Sand/grus", "Sand/grus", null, null));
    }

    [Fact]
    public void TomtBehov_TrefferIkke()
    {
        Assert.False(BehovRessursTreff.Matcher("  ", "Lastebil", null, null));
    }
}
