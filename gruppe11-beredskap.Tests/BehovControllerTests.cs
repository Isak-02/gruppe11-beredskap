using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.Controllers;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.Tests;

// Falskt repository i minnet, så testene kjører uten database.
public class FakeBehovRepository : IBehovRepository
{
    public List<Behov> Lagret { get; } = new();

    public Task AddAsync(Behov behov)
    {
        behov.Id = Lagret.Count + 1;
        Lagret.Add(behov);
        return Task.CompletedTask;
    }

    public Task<List<Behov>> GetAllAsync()
    {
        return Task.FromResult(Lagret.ToList());
    }
}

public class BehovControllerTests
{
    // Koordinater må settes, fordi controlleren kalles direkte uten [Required]-validering
    private static BehovViewModel LagModel() => new()
    {
        TypeBehov = "Transport",
        Prioritet = "Høy",
        Tidspunkt = DateTime.Now,
        Kontaktpunkt = "Testperson",
        Latitude = 58.14,
        Longitude = 7.99
    };

    [Fact]
    public void Registrer_Get_ReturnsViewResult()
    {
        var controller = new BehovController(new FakeBehovRepository());

        var result = controller.Registrer();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Registrer_Post_WithValidModel_RedirectsToOversikt()
    {
        var controller = new BehovController(new FakeBehovRepository());

        var result = await controller.Registrer(LagModel());

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Oversikt", redirect.ActionName);
    }

    [Fact]
    public async Task Registrer_Post_LagrerBehovIRepository()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);

        await controller.Registrer(LagModel());

        var lagret = Assert.Single(repo.Lagret);
        Assert.Equal("Transport", lagret.TypeBehov);
        Assert.Equal("Høy", lagret.Prioritet);
        Assert.Equal(58.14, lagret.Latitude);
        Assert.True(lagret.Id > 0);
    }

    [Fact]
    public async Task Registrer_Post_UgyldigModell_ReturnsViewResultOgLagrerIkke()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);
        controller.ModelState.AddModelError("Latitude", "Du må tegne en markør eller sirkel på kartet for å velge sted");

        var result = await controller.Registrer(LagModel());

        Assert.IsType<ViewResult>(result);
        Assert.Empty(repo.Lagret);
    }

    [Fact]
    public async Task Oversikt_ReturnsViewResult_MedLagredeBehov()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);
        await controller.Registrer(LagModel());

        var result = await controller.Oversikt();

        var view = Assert.IsType<ViewResult>(result);
        var liste = Assert.IsType<List<BehovViewModel>>(view.Model);
        Assert.Single(liste);
    }

    [Fact]
    public async Task Registrer_Post_MedSirkel_LagrerRadius()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);

        var model = LagModel();
        model.Radius = 250;

        await controller.Registrer(model);

        var lagret = Assert.Single(repo.Lagret);
        Assert.Equal(250, lagret.Radius);
    }

    [Fact]
    public async Task Registrer_Post_MedMarkør_LagrerRadiusSomNull()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);

        await controller.Registrer(LagModel());

        var lagret = Assert.Single(repo.Lagret);
        Assert.Null(lagret.Radius);
    }

    [Fact]
    public async Task Oversikt_ViserRadiusFraLagretBehov()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);

        var model = LagModel();
        model.Radius = 250;
        await controller.Registrer(model);

        var result = await controller.Oversikt();

        var view = Assert.IsType<ViewResult>(result);
        var liste = Assert.IsType<List<BehovViewModel>>(view.Model);
        var behov = Assert.Single(liste);
        Assert.Equal(250, behov.Radius);
    }
}
