using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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

    public Task UpdateStatusAsync(int id, string status)
    {
        var behov = Lagret.FirstOrDefault(b => b.Id == id);
        if (behov != null)
            behov.Status = status;
        return Task.CompletedTask;
    }
}

public class BehovControllerTests
{
    // Koordinater må settes, fordi controlleren kalles direkte uten [Required]-validering
    private static BehovViewModel LagModel() => new()
    {
        TypeBehov = "Transport",
        Beskrivelse = "Trenger transport av utstyr",
        Prioritet = "Høy",
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
    public async Task Registrer_Post_WithValidModel_RedirectsToMine()
    {
        var controller = new BehovController(new FakeBehovRepository());

        var result = await controller.Registrer(LagModel());

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Mine", redirect.ActionName);
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

    [Fact]
    public async Task Registrer_Post_LagrerBeskrivelse()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);

        var model = LagModel();
        model.Beskrivelse = "Trenger aggregat til skolen";

        await controller.Registrer(model);

        var lagret = Assert.Single(repo.Lagret);
        Assert.Equal("Trenger aggregat til skolen", lagret.Beskrivelse);
    }

    [Fact]
    public async Task Oversikt_ViserBeskrivelseFraLagretBehov()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);
        await controller.Registrer(LagModel());

        var result = await controller.Oversikt();

        var view = Assert.IsType<ViewResult>(result);
        var liste = Assert.IsType<List<BehovViewModel>>(view.Model);
        var behov = Assert.Single(liste);
        Assert.Equal("Trenger transport av utstyr", behov.Beskrivelse);
    }

    // Later som om en bruker med denne Id-en er logget inn, slik controlleren ser det
    private static void LoggInnSom(Controller controller, string brukerId)
    {
        var bruker = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, brukerId) }, "Test"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = bruker }
        };
    }

    [Fact]
    public async Task Registrer_Post_LagrerInnloggetBrukerSomOppretter()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);
        LoggInnSom(controller, "bruker-1");

        await controller.Registrer(LagModel());

        var lagret = Assert.Single(repo.Lagret);
        Assert.Equal("bruker-1", lagret.OpprettetAvId);
    }

    [Fact]
    public async Task Mine_ViserBareBehovSomBrukerenSelvHarRegistrert()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);

        LoggInnSom(controller, "bruker-1");
        await controller.Registrer(LagModel());
        LoggInnSom(controller, "bruker-2");
        await controller.Registrer(LagModel());

        LoggInnSom(controller, "bruker-1");
        var result = await controller.Mine();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Oversikt", view.ViewName);
        var liste = Assert.IsType<List<BehovViewModel>>(view.Model);
        var behov = Assert.Single(liste);
        Assert.Equal(1, behov.Id);
    }

    [Fact]
    public async Task Mine_UtenInnloggetBruker_ViserIngenBehov()
    {
        var repo = new FakeBehovRepository();
        var controller = new BehovController(repo);
        // Behov uten "opprettet av", slik som de som ble registrert før innlogging fantes
        await controller.Registrer(LagModel());

        var result = await controller.Mine();

        var view = Assert.IsType<ViewResult>(result);
        var liste = Assert.IsType<List<BehovViewModel>>(view.Model);
        Assert.Empty(liste);
    }
}
