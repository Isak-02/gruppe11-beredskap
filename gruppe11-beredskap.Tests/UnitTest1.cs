using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.Controllers;
using gruppe11_beredskap.Models;

namespace gruppe11_beredskap.Tests;

public class RessursControllerTests
{
    [Fact]
    public void Registrer_Get_ReturnsViewResult()
    {
        var controller = new RessursController();

        var result = controller.Registrer();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Registrer_Post_WithValidModel_RedirectsToOversikt()
    {
        var controller = new RessursController();

        var model = new RessursViewModel
        {
            TypeRessurs = "Mat",
            Tidspunkt = DateTime.Now,
            Kontaktpunkt = "Testperson"
        };

        var result = controller.Registrer(model);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Oversikt", redirectResult.ActionName);
    }

    [Fact]
    public void Oversikt_ReturnsViewResult()
    {
        var controller = new RessursController();

        var result = controller.Oversikt();

        Assert.IsType<ViewResult>(result);
    }
}