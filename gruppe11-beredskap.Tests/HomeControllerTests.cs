using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.Controllers;
using gruppe11_beredskap.Models.Authorization;

namespace gruppe11_beredskap.Tests;

// Sjekker at forsiden bare vises for den som ikke er logget inn,
// og at innloggede sendes videre til riktig side etter rolle.
public class HomeControllerTests
{
    // Lager en controller der brukeren har den gitte rollen (eller ingen innlogging når rolle er null)
    private static HomeController LagController(string? rolle)
    {
        var identitet = rolle == null
            ? new ClaimsIdentity()
            : new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, rolle) }, "Test");

        return new HomeController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identitet) }
            }
        };
    }

    [Fact]
    public void Index_IkkeInnlogget_ViserForsiden()
    {
        var result = LagController(null).Index();

        Assert.IsType<ViewResult>(result);
    }

    [Theory]
    [InlineData(RoleNames.Koordinator, "Matching", "Index")]
    [InlineData(RoleNames.KriseInnmelder, "Behov", "Mine")]
    [InlineData(RoleNames.RessursInnmelder, "Ressurs", "Mine")]
    public void Index_Innlogget_SendesVidereEtterRolle(string rolle, string controller, string handling)
    {
        var result = LagController(rolle).Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(controller, redirect.ControllerName);
        Assert.Equal(handling, redirect.ActionName);
    }
}
