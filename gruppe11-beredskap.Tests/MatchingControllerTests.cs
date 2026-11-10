using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.Controllers;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.Tests;

// Falskt repository i minnet, så testene kjører uten database.
public class FakeMatchingRepository : IMatchingRepository
{
    public List<Matching> Lagret { get; } = new();

    public Task AddAsync(Matching matching)
    {
        matching.Id = Lagret.Count + 1;
        Lagret.Add(matching);
        return Task.CompletedTask;
    }

    public Task<List<Matching>> GetAllAsync()
    {
        return Task.FromResult(Lagret.ToList());
    }

    public Task<HashSet<int>> GetMatchedeBehovIdsAsync()
    {
        return Task.FromResult(Lagret.Select(m => m.BehovId).ToHashSet());
    }

    public Task<HashSet<int>> GetMatchedeRessursIdsAsync()
    {
        return Task.FromResult(Lagret.Select(m => m.RessursId).ToHashSet());
    }

    public Task DeleteByBehovIdAsync(int behovId)
    {
        Lagret.RemoveAll(m => m.BehovId == behovId);
        return Task.CompletedTask;
    }

    public Task MarkerLostAsync(int behovId)
    {
        foreach (var m in Lagret.Where(m => m.BehovId == behovId && m.LostTidspunkt == null))
            m.LostTidspunkt = DateTime.UtcNow;
        return Task.CompletedTask;
    }
}

public class MatchingControllerTests
{
    // Behov og ressurser legges rett i de falske repositoriene, med faste Id-er og posisjoner
    private static Behov LagBehov(int id) => new()
    {
        Id = id,
        TypeBehov = "Transport",
        Beskrivelse = "Trenger transport av utstyr",
        Prioritet = "Høy",
        Tidspunkt = DateTime.Now,
        Kontaktpunkt = "Testperson",
        Latitude = 58.14,
        Longitude = 7.99
    };

    private static Ressurs LagRessurs(int id) => new()
    {
        Id = id,
        TypeRessurs = "Traktor",
        Beskrivelse = "Kan brukes hele døgnet",
        Tidspunkt = DateTime.Now,
        Kontaktpunkt = "Testperson",
        Latitude = 58.20,
        Longitude = 8.10
    };

    [Fact]
    public async Task Index_ViserBareÅpneBehovOgLedigeRessurser()
    {
        var behovRepo = new FakeBehovRepository();
        var ressursRepo = new FakeRessursRepository();
        var matchingRepo = new FakeMatchingRepository();
        behovRepo.Lagret.Add(LagBehov(1));
        behovRepo.Lagret[0].Status = BehovStatus.Løst;
        behovRepo.Lagret.Add(LagBehov(2));
        ressursRepo.Lagret.Add(LagRessurs(1));
        ressursRepo.Lagret[0].Status = RessursStatus.Tildelt;
        ressursRepo.Lagret.Add(LagRessurs(2));
        matchingRepo.Lagret.Add(new Matching { Id = 1, BehovId = 1, RessursId = 1 });
        var controller = new MatchingController(behovRepo, ressursRepo, matchingRepo);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MatchingViewModel>(view.Model);
        var åpentBehov = Assert.Single(model.ÅpneBehov);
        Assert.Equal(2, åpentBehov.Id);
        var ledigRessurs = Assert.Single(model.LedigeRessurser);
        Assert.Equal(2, ledigRessurs.Id);
        Assert.Single(model.Matchinger);
    }

    [Fact]
    public async Task Index_SenderMedKoordinaterTilKartet()
    {
        var behovRepo = new FakeBehovRepository();
        var ressursRepo = new FakeRessursRepository();
        behovRepo.Lagret.Add(LagBehov(1));
        ressursRepo.Lagret.Add(LagRessurs(1));
        var controller = new MatchingController(behovRepo, ressursRepo, new FakeMatchingRepository());

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MatchingViewModel>(view.Model);
        var behov = Assert.Single(model.ÅpneBehov);
        Assert.Equal(58.14, behov.Latitude);
        Assert.Equal(7.99, behov.Longitude);
        var ressurs = Assert.Single(model.LedigeRessurser);
        Assert.Equal(58.20, ressurs.Latitude);
        Assert.Equal(8.10, ressurs.Longitude);
    }

    [Fact]
    public async Task Koble_Post_GyldigValg_LagrerKoblingOgSenderTilIndex()
    {
        var behovRepo = new FakeBehovRepository();
        var ressursRepo = new FakeRessursRepository();
        var matchingRepo = new FakeMatchingRepository();
        behovRepo.Lagret.Add(LagBehov(1));
        ressursRepo.Lagret.Add(LagRessurs(1));
        var controller = new MatchingController(behovRepo, ressursRepo, matchingRepo);

        var result = await controller.Koble(new MatchingViewModel { BehovId = 1, RessursIder = new() { 1 } });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        var lagret = Assert.Single(matchingRepo.Lagret);
        Assert.Equal(1, lagret.BehovId);
        Assert.Equal(1, lagret.RessursId);
    }

    [Fact]
    public async Task Koble_Post_UtenValg_ReturnsViewResultOgLagrerIkke()
    {
        var matchingRepo = new FakeMatchingRepository();
        var controller = new MatchingController(new FakeBehovRepository(), new FakeRessursRepository(), matchingRepo);
        controller.ModelState.AddModelError("BehovId", "Du må velge et behov");

        var result = await controller.Koble(new MatchingViewModel());

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        Assert.Empty(matchingRepo.Lagret);
    }

    [Fact]
    public async Task Koble_Post_FlereRessurser_LagerÉnKoblingPerRessurs()
    {
        var behovRepo = new FakeBehovRepository();
        var ressursRepo = new FakeRessursRepository();
        var matchingRepo = new FakeMatchingRepository();
        behovRepo.Lagret.Add(LagBehov(1));
        ressursRepo.Lagret.Add(LagRessurs(1));
        ressursRepo.Lagret.Add(LagRessurs(2));
        var controller = new MatchingController(behovRepo, ressursRepo, matchingRepo);

        await controller.Koble(new MatchingViewModel { BehovId = 1, RessursIder = new() { 1, 2 } });

        Assert.Equal(2, matchingRepo.Lagret.Count);
        Assert.All(matchingRepo.Lagret, m => Assert.Equal(1, m.BehovId));
        Assert.All(ressursRepo.Lagret, r => Assert.Equal(RessursStatus.Tildelt, r.Status));
        Assert.Equal(BehovStatus.Tildelt, behovRepo.Lagret[0].Status);
    }

    [Fact]
    public async Task Koble_Post_TildeltBehov_KanFåFlereRessurserSenere()
    {
        var behovRepo = new FakeBehovRepository();
        var ressursRepo = new FakeRessursRepository();
        var matchingRepo = new FakeMatchingRepository();
        behovRepo.Lagret.Add(LagBehov(1));
        ressursRepo.Lagret.Add(LagRessurs(1));
        ressursRepo.Lagret.Add(LagRessurs(2));
        var controller = new MatchingController(behovRepo, ressursRepo, matchingRepo);
        await controller.Koble(new MatchingViewModel { BehovId = 1, RessursIder = new() { 1 } });

        var result = await controller.Koble(new MatchingViewModel { BehovId = 1, RessursIder = new() { 2 } });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(2, matchingRepo.Lagret.Count);
    }

    [Fact]
    public async Task Koble_Post_RessursSomIkkeErLedig_LagrerIkke()
    {
        var behovRepo = new FakeBehovRepository();
        var ressursRepo = new FakeRessursRepository();
        var matchingRepo = new FakeMatchingRepository();
        behovRepo.Lagret.Add(LagBehov(1));
        ressursRepo.Lagret.Add(LagRessurs(1));
        ressursRepo.Lagret.Add(LagRessurs(2));
        ressursRepo.Lagret[1].Status = RessursStatus.Tildelt;
        var controller = new MatchingController(behovRepo, ressursRepo, matchingRepo);

        var result = await controller.Koble(new MatchingViewModel { BehovId = 1, RessursIder = new() { 1, 2 } });

        Assert.IsType<ViewResult>(result);
        Assert.Empty(matchingRepo.Lagret);
    }

    [Fact]
    public async Task Løs_GjørAlleRessurseneLedigeOgBehovetLøst()
    {
        var behovRepo = new FakeBehovRepository();
        var ressursRepo = new FakeRessursRepository();
        var matchingRepo = new FakeMatchingRepository();
        behovRepo.Lagret.Add(LagBehov(1));
        ressursRepo.Lagret.Add(LagRessurs(1));
        ressursRepo.Lagret.Add(LagRessurs(2));
        var controller = new MatchingController(behovRepo, ressursRepo, matchingRepo);
        await controller.Koble(new MatchingViewModel { BehovId = 1, RessursIder = new() { 1, 2 } });

        await controller.Løs(1);

        Assert.Equal(BehovStatus.Løst, behovRepo.Lagret[0].Status);
        Assert.All(ressursRepo.Lagret, r => Assert.Equal(RessursStatus.Tilgjengelig, r.Status));
        Assert.All(matchingRepo.Lagret, m => Assert.NotNull(m.LostTidspunkt));
    }

    [Fact]
    public async Task Index_SenderMedBeskrivelseAvBehov()
    {
        var behovRepo = new FakeBehovRepository();
        behovRepo.Lagret.Add(LagBehov(1));
        var controller = new MatchingController(behovRepo, new FakeRessursRepository(), new FakeMatchingRepository());

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MatchingViewModel>(view.Model);
        var behov = Assert.Single(model.ÅpneBehov);
        Assert.Equal("Trenger transport av utstyr", behov.Beskrivelse);
    }
}
