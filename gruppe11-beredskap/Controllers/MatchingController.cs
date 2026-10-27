using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.Controllers;

// Matching av behov og ressurser. Skal bare være tilgjengelig for kriseleder
// (tilgangskontroll legges på når Identity er på plass).
public class MatchingController : Controller
{
    private readonly IBehovRepository _behovRepo;
    private readonly IRessursRepository _ressursRepo;
    private readonly IMatchingRepository _matchingRepo;

    public MatchingController(
        IBehovRepository behovRepo,
        IRessursRepository ressursRepo,
        IMatchingRepository matchingRepo)
    {
        _behovRepo = behovRepo;
        _ressursRepo = ressursRepo;
        _matchingRepo = matchingRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await ByggModell(new MatchingViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Koble(MatchingViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Index", await ByggModell(model));

        var matching = new Matching
        {
            BehovId = model.BehovId!.Value,
            RessursId = model.RessursId!.Value
        };

        await _matchingRepo.AddAsync(matching);
        return RedirectToAction(nameof(Index));
    }

    // Fyller listene: behov og ressurser som ikke allerede er koblet, pluss eksisterende koblinger
    private async Task<MatchingViewModel> ByggModell(MatchingViewModel model)
    {
        var matchedeBehov = await _matchingRepo.GetMatchedeBehovIdsAsync();
        var matchedeRessurser = await _matchingRepo.GetMatchedeRessursIdsAsync();

        var behov = await _behovRepo.GetAllAsync();
        var ressurser = await _ressursRepo.GetAllAsync();
        var matchinger = await _matchingRepo.GetAllAsync();

        model.ÅpneBehov = behov
            .Where(b => !matchedeBehov.Contains(b.Id))
            .Select(b => new BehovViewModel
            {
                Id = b.Id,
                TypeBehov = b.TypeBehov,
                Prioritet = b.Prioritet,
                GeografiskOmrade = b.GeografiskOmrade
            }).ToList();

        model.LedigeRessurser = ressurser
            .Where(r => !matchedeRessurser.Contains(r.Id))
            .Select(r => new RessursViewModel
            {
                Id = r.Id,
                TypeRessurs = r.TypeRessurs,
                AnnenType = r.AnnenType,
                GeografiskOmrade = r.GeografiskOmrade
            }).ToList();

        model.Matchinger = matchinger.Select(m => new MatchingVisning
        {
            Id = m.Id,
            Behov = $"#{m.BehovId} {m.Behov?.TypeBehov}",
            Ressurs = $"#{m.RessursId} {m.Ressurs?.TypeRessurs}",
            OpprettetTidspunkt = m.OpprettetTidspunkt
        }).ToList();

        return model;
    }
}
