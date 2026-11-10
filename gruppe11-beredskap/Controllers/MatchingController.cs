using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Authorization;
using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.Controllers;

// Matching (kobling) av behov og ressurser. Bare koordinatorer har tilgang til hele controlleren.
[Authorize(Roles = RoleNames.Koordinator)]
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

        var behovene = await _behovRepo.GetAllAsync();
        var ressursene = await _ressursRepo.GetAllAsync();
        var valgtBehov = behovene.FirstOrDefault(b => b.Id == model.BehovId);
        var valgteRessurser = ressursene.Where(r => model.RessursIder.Contains(r.Id)).ToList();

        // Et behov kan kobles så lenge det ikke er løst. Et tildelt behov kan altså få flere ressurser senere.
        if (valgtBehov == null || valgtBehov.Status == BehovStatus.Løst)
            ModelState.AddModelError(nameof(model.BehovId), "Behovet kan ikke kobles");
        // Alle de valgte ressursene må finnes og være ledige
        if (valgteRessurser.Count != model.RessursIder.Distinct().Count() ||
            valgteRessurser.Any(r => r.Status != RessursStatus.Tilgjengelig))
            ModelState.AddModelError(nameof(model.RessursIder), "En eller flere av ressursene er ikke ledige");

        if (!ModelState.IsValid)
            return View("Index", await ByggModell(model));

        // Én kobling per ressurs, alle til samme behov
        foreach (var ressurs in valgteRessurser)
        {
            await _matchingRepo.AddAsync(new Matching
            {
                BehovId = valgtBehov!.Id,
                RessursId = ressurs.Id
            });
            await _ressursRepo.UpdateStatusAsync(ressurs.Id, RessursStatus.Tildelt);
        }

        await _behovRepo.UpdateStatusAsync(valgtBehov!.Id, BehovStatus.Tildelt);
        return RedirectToAction(nameof(Index));
    }
    // Behov går fra Ny til Vurderes
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Vurder(int behovId)
    {
        await _behovRepo.UpdateStatusAsync(behovId, BehovStatus.Vurderes);
        return RedirectToAction(nameof(Index));
    }

    // Behov er løst: alle ressursene som var koblet til det, blir ledige igjen
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Løs(int behovId)
    {
        var matchinger = await _matchingRepo.GetAllAsync();
        var aktiveKoblinger = matchinger.Where(m => m.BehovId == behovId && m.LostTidspunkt == null);

        foreach (var kobling in aktiveKoblinger)
            await _ressursRepo.UpdateStatusAsync(kobling.RessursId, RessursStatus.Tilgjengelig);

        await _behovRepo.UpdateStatusAsync(behovId, BehovStatus.Løst);
        await _matchingRepo.MarkerLostAsync(behovId);
        return RedirectToAction(nameof(Index));
    }

    // Fyller listene: behov som ikke er løst (også tildelte, så de kan få flere ressurser),
    // ledige ressurser og eksisterende koblinger
    private async Task<MatchingViewModel> ByggModell(MatchingViewModel model)
    {

        var behov = await _behovRepo.GetAllAsync();
        var ressurser = await _ressursRepo.GetAllAsync();
        var matchinger = await _matchingRepo.GetAllAsync();

        model.ÅpneBehov = behov
            .Where(b => b.Status != BehovStatus.Løst)
            .Select(b => new BehovViewModel
            {
                Id = b.Id,
                Status = b.Status,
                TypeBehov = b.TypeBehov,
                Beskrivelse = b.Beskrivelse,
                Prioritet = b.Prioritet,
                GeografiskOmrade = b.GeografiskOmrade,
                Tidspunkt = b.Tidspunkt,
                Kontaktpunkt = b.Kontaktpunkt,
                Latitude = b.Latitude,
                Longitude = b.Longitude
            }).ToList();

        model.LedigeRessurser = ressurser
            .Where(r => r.Status == RessursStatus.Tilgjengelig)
            .Select(r => new RessursViewModel
            {
                Id = r.Id,
                Status = r.Status,
                TypeRessurs = r.TypeRessurs,
                Beskrivelse = r.Beskrivelse,
                AnnenType = r.AnnenType,
                Drivstofftype = r.Drivstofftype,
                Lastekapasitet = r.Lastekapasitet,
                GeografiskOmrade = r.GeografiskOmrade,
                Tidspunkt = r.Tidspunkt,
                Kontaktpunkt = r.Kontaktpunkt,
                Latitude = r.Latitude,
                Longitude = r.Longitude
            }).ToList();

        model.Matchinger = matchinger.Select(m => new MatchingVisning
        {
            Id = m.Id,
            BehovId = m.BehovId,
            RessursId = m.RessursId,
            Behov = $"#{m.BehovId} {m.Behov?.TypeBehov}",
            Ressurs = $"#{m.RessursId} {m.Ressurs?.TypeRessurs}",
            OpprettetTidspunkt = m.OpprettetTidspunkt,
            LostTidspunkt = m.LostTidspunkt
        }).ToList();

        return model;
    }
}
