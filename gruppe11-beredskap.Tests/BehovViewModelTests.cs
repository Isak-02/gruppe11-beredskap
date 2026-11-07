using System.ComponentModel.DataAnnotations;
using gruppe11_beredskap.Models;

namespace gruppe11_beredskap.Tests;

// Tester valideringen i BehovViewModel ([Required] og [Range]).
// Validator kjører de samme attributtene som ASP.NET sjekker når skjemaet sendes inn.
public class BehovViewModelTests
{
    private static BehovViewModel LagModel() => new()
    {
        TypeBehov = "Transport",
        Beskrivelse = "Trenger transport av utstyr",
        Prioritet = "Høy",
        Tidspunkt = DateTime.Now,
        Kontaktpunkt = "Testperson",
        Latitude = 58.14,
        Longitude = 7.99
    };

    private static List<ValidationResult> Valider(BehovViewModel model)
    {
        var feil = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), feil, validateAllProperties: true);
        return feil;
    }

    [Fact]
    public void Radius_ErValgfri_MarkørUtenRadiusErGyldig()
    {
        var model = LagModel();

        var feil = Valider(model);

        Assert.Empty(feil);
    }

    [Fact]
    public void Radius_StørreEnnNull_ErGyldig()
    {
        var model = LagModel();
        model.Radius = 250;

        var feil = Valider(model);

        Assert.Empty(feil);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-50.0)]
    public void Radius_NullEllerNegativ_GirFeilmelding(double radius)
    {
        var model = LagModel();
        model.Radius = radius;

        var feil = Valider(model);

        var radiusFeil = Assert.Single(feil);
        Assert.Contains("Radius", radiusFeil.MemberNames);
        Assert.Equal("Radius må være mellom 1 og 50 000 meter", radiusFeil.ErrorMessage);
    }

    [Fact]
    public void Radius_Over50000Meter_GirFeilmelding()
    {
        var model = LagModel();
        model.Radius = 50001;

        var feil = Valider(model);

        var radiusFeil = Assert.Single(feil);
        Assert.Equal("Radius må være mellom 1 og 50 000 meter", radiusFeil.ErrorMessage);
    }

    [Fact]
    public void UtenPosisjon_GirFeilmeldingOmÅTegnePåKartet()
    {
        var model = LagModel();
        model.Latitude = null;
        model.Longitude = null;

        var feil = Valider(model);

        Assert.Equal(2, feil.Count);
        Assert.All(feil, f => Assert.Equal("Du må tegne en markør eller sirkel på kartet for å velge sted", f.ErrorMessage));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UtenBeskrivelse_GirFeilmelding(string beskrivelse)
    {
        var model = LagModel();
        model.Beskrivelse = beskrivelse;

        var feil = Valider(model);

        var beskrivelseFeil = Assert.Single(feil);
        Assert.Contains("Beskrivelse", beskrivelseFeil.MemberNames);
        Assert.Equal("Du må skrive en beskrivelse av behovet", beskrivelseFeil.ErrorMessage);
    }

    [Fact]
    public void Beskrivelse_LengreEnn1000Tegn_GirFeilmelding()
    {
        var model = LagModel();
        model.Beskrivelse = new string('a', 1001);

        var feil = Valider(model);

        var beskrivelseFeil = Assert.Single(feil);
        Assert.Equal("Beskrivelsen kan være maks 1000 tegn", beskrivelseFeil.ErrorMessage);
    }
}
