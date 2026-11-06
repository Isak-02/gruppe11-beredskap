using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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

    public Task<Ressurs?> GetByIdAsync(int id)
    {
        return Task.FromResult(Lagret.FirstOrDefault(r => r.Id == id));
    }

    public Task UpdateAsync(Ressurs ressurs)
    {
        var index = Lagret.FindIndex(r => r.Id == ressurs.Id);
        if (index >= 0)
            Lagret[index] = ressurs;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Lagret.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
    }

    public Task UpdateStatusAsync(int id, string status)
    {
        var ressurs = Lagret.FirstOrDefault(r => r.Id == id);
        if (ressurs != null)
            ressurs.Status = status;
        return Task.CompletedTask;
    }
}

public class RessursControllerTests
{
    // Koordinater må settes, fordi controlleren kalles direkte uten [Required]-validering
    private static RessursViewModel LagModel(string type) => new()
    {
        TypeRessurs = type,
        Beskrivelse = "Kan brukes hele døgnet",
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
    public async Task Registrer_Post_WithValidModel_RedirectsToMine()
    {
        var controller = new RessursController(new FakeRessursRepository());

        var result = await controller.Registrer(LagModel("Mat"));

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Mine", redirectResult.ActionName);
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
        var repo = new FakeRessursRepository();
        var controller = new RessursController(repo);
        LoggInnSom(controller, "bruker-1");

        await controller.Registrer(LagModel("Mat"));

        var lagret = Assert.Single(repo.Lagret);
        Assert.Equal("bruker-1", lagret.OpprettetAvId);
    }

    [Fact]
    public async Task Mine_ViserBareRessurserSomBrukerenSelvHarRegistrert()
    {
        var repo = new FakeRessursRepository();
        var controller = new RessursController(repo);

        LoggInnSom(controller, "bruker-1");
        await controller.Registrer(LagModel("Mat"));
        LoggInnSom(controller, "bruker-2");
        await controller.Registrer(LagModel("Mat"));

        LoggInnSom(controller, "bruker-1");
        var result = await controller.Mine();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Oversikt", view.ViewName);
        var liste = Assert.IsType<List<RessursViewModel>>(view.Model);
        var ressurs = Assert.Single(liste);
        Assert.Equal(1, ressurs.Id);
    }

    [Fact]
    public async Task Endre_Post_BytterType()
    {
        var repo = new FakeRessursRepository();
        var controller = new RessursController(repo);
        LoggInnSom(controller, "bruker-1");
        await controller.Registrer(LagModel("Mat"));

        var model = LagModel("Lokaler");
        model.Id = 1;
        await controller.Endre(model);

        Assert.Equal("Lokaler", repo.Lagret[0].TypeRessurs);
    }

    [Fact]
    public async Task Slett_FjernerRaden()
    {
        var repo = new FakeRessursRepository();
        var controller = new RessursController(repo);
        LoggInnSom(controller, "bruker-1");
        await controller.Registrer(LagModel("Mat"));

        await controller.Slett(1);

        Assert.Empty(repo.Lagret);
    }

    [Fact]
    public async Task PerFylke_ViserBareTilgjengeligeRessurser()
    {
        var repo = new FakeRessursRepository();
        var controller = new RessursController(repo);
        await controller.Registrer(LagModel("Mat"));
        await controller.Registrer(LagModel("Lokaler"));
        repo.Lagret[1].Status = RessursStatus.Tildelt;

        var result = await controller.PerFylke();

        var view = Assert.IsType<ViewResult>(result);
        var liste = Assert.IsType<List<RessursViewModel>>(view.Model);
        Assert.Single(liste);
        Assert.Equal("Mat", liste[0].TypeRessurs);
    }
}
