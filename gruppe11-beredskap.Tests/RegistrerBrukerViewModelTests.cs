using System.ComponentModel.DataAnnotations;
using gruppe11_beredskap.Models;

namespace gruppe11_beredskap.Tests;

// Tester valideringen i skjemaet "Opprett bruker". Navn, e-post og telefon er påkrevd,
// fordi de brukes som kontaktpunkt når brukeren registrerer behov og ressurser.
public class RegistrerBrukerViewModelTests
{
    private static RegistrerBrukerViewModel LagModel() => new()
    {
        Navn = "Ola Nordmann",
        Epost = "ola@eksempel.no",
        Telefon = "12345678",
        Passord = "Passord123!",
        BekreftPassord = "Passord123!"
    };

    private static List<ValidationResult> Valider(RegistrerBrukerViewModel model)
    {
        var feil = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), feil, validateAllProperties: true);
        return feil;
    }

    [Fact]
    public void GyldigModell_GirIngenFeil()
    {
        Assert.Empty(Valider(LagModel()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Telefon_Mangler_GirFeilmelding(string telefon)
    {
        var model = LagModel();
        model.Telefon = telefon;

        var feil = Assert.Single(Valider(model));

        Assert.Contains("Telefon", feil.MemberNames);
        Assert.Equal("Du må skrive inn telefonnummeret ditt", feil.ErrorMessage);
    }
}
