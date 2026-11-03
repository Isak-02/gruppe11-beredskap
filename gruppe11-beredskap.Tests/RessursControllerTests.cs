using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.Controllers;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.Tests;

// Falskt repository i minnet, så testene kjører uten database.
public class FakeRessursRepository : IRessursRepository
{
    public List<Ressurs> Lagret { get; } = new();

    public Task AddAsync(Ressurs ressurs)
    {
        ressurs.Id = Lagret.Count + 1;
        Lagret.Add(ressurs);
        return Task.CompletedTask;
    }

    public Task<List<Ressurs>> GetAllAsync()
    {
        return Task.FromResult(Lagret.ToList());
    }
}

public class RessursControllerTests
{
    // Koordinater må settes, fordi controlleren kalles direkte uten [Required]-validering
    private static RessursViewModel LagModel(string type) => new()
    {
        TypeRessurs = type,
        Tidspunkt = DateTime.Now,
        Kontaktpunkt = "Testperson",
        Latitude = 58.14,
        Longitude = 7.99
    };

    [Fact]
    public void Registrer_Get_ReturnsViewResult()
    {
        var controller = new RessursController(new FakeRessursRepository());

        var result = controller.Registrer();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Registrer_Post_WithValidModel_RedirectsToOversikt()
    {
        var controller = new RessursController(new FakeRessursRepository());

        var result = await controller.Registrer(LagModel("Mat"));

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Oversikt", redirectResult.ActionName);
    }

    [Fact]
    public async Task Registrer_Post_LagrerRessursIRepository()
    {
        var repo = new FakeRessursRepository();
        var controller = new RessursController(repo);

        var model = LagModel("Gravemaskin");
        model.Drivstofftype = "Diesel";
        model.Lastekapasitet = 2;

        await controller.Registrer(model);

        var lagret = Assert.Single(repo.Lagret);
        Assert.Equal("Gravemaskin", lagret.TypeRessurs);
        Assert.Equal(58.14, lagret.Latitude);
        Assert.True(lagret.Id > 0);
    }

    [Fact]
    public async Task Registrer_Post_KjøretøyUtenDrivstoff_ReturnsViewResult()
    {
        var repo = new FakeRessursRepository();
        var controller = new RessursController(repo);

        var result = await controller.Registrer(LagModel("Lastebil"));

        Assert.IsType<ViewResult>(result);
        Assert.Empty(repo.Lagret);
    }

    [Fact]
    public async Task Registrer_Post_LastebilUtenLastekapasitet_ReturnsViewResult()
    {
        var repo = new FakeRessursRepository();
        var controller = new RessursController(repo);

        var model = LagModel("Lastebil");
        model.Drivstofftype = "Diesel";

        var result = await controller.Registrer(model);

        Assert.IsType<ViewResult>(result);
        Assert.Empty(repo.Lagret);
    }

    [Fact]
    public async Task Oversikt_ReturnsViewResult_MedLagredeRessurser()
    {
        var repo = new FakeRessursRepository();
        var controller = new RessursController(repo);
        await controller.Registrer(LagModel("Mat"));

        var result = await controller.Oversikt();

        var view = Assert.IsType<ViewResult>(result);
        var liste = Assert.IsType<List<RessursViewModel>>(view.Model);
        Assert.Single(liste);
    }
}
