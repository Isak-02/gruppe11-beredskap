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
        Assert.Equal("Radius må være større enn 0 meter", radiusFeil.ErrorMessage);
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
}
