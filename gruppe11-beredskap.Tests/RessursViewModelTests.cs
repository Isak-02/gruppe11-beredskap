using System.ComponentModel.DataAnnotations;
using gruppe11_beredskap.Models;

namespace gruppe11_beredskap.Tests;

// Tester grensene i RessursViewModel ([Range] og [StringLength]).
// Validator kjører de samme attributtene som ASP.NET sjekker når skjemaet sendes inn.
public class RessursViewModelTests
{
    private static RessursViewModel LagModel() => new()
    {
        TypeRessurs = "Lastebil",
        Beskrivelse = "Kan brukes hele døgnet",
        Kontaktpunkt = "Testperson",
        Latitude = 58.14,
        Longitude = 7.99
    };

    private static List<ValidationResult> Valider(RessursViewModel model)
    {
        var feil = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), feil, validateAllProperties: true);
        return feil;
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(25.5)]
    [InlineData(60.0)]
    public void Lastekapasitet_Innenfor0_01Og60Tonn_ErGyldig(double tonn)
    {
        var model = LagModel();
        model.Lastekapasitet = (decimal)tonn;

        var feil = Valider(model);

        Assert.Empty(feil);
    }

    [Fact]
    public void Lastekapasitet_Over60Tonn_GirFeilmelding()
    {
        var model = LagModel();
        model.Lastekapasitet = 60.01m;

        var feil = Valider(model);

        var lastFeil = Assert.Single(feil);
        Assert.Contains("Lastekapasitet", lastFeil.MemberNames);
        Assert.Equal("Lastekapasiteten må være mellom 0,01 og 60 tonn", lastFeil.ErrorMessage);
    }

    [Fact]
    public void AnnenType_LengreEnn200Tegn_GirFeilmelding()
    {
        var model = LagModel();
        model.TypeRessurs = "Annet";
        model.AnnenType = new string('a', 201);

        var feil = Valider(model);

        var typeFeil = Assert.Single(feil);
        Assert.Contains("AnnenType", typeFeil.MemberNames);
        Assert.Equal("Typen kan være maks 200 tegn", typeFeil.ErrorMessage);
    }
}
