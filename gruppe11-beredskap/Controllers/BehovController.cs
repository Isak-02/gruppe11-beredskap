using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Authorization;
using gruppe11_beredskap.Models.Entities;
using gruppe11_beredskap.Services;

namespace gruppe11_beredskap.Controllers;

// Håndterer innmelding og visning av behov. Lagrer i MariaDB via repository.
// Krise-innmeldere og koordinatorer kan registrere behov og se sine egne. Bare koordinatorer ser alle.
public class BehovController : Controller
{
    private readonly IBehovRepository _repo;
    private readonly IKontaktinfoTjeneste _kontaktinfo;

    public BehovController(IBehovRepository repo, IKontaktinfoTjeneste kontaktinfo)
    {
        _repo = repo;
        _kontaktinfo = kontaktinfo;
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.KanRegistrereBehov)]
    public async Task<IActionResult> Registrer()
    {
        var kontakt = await _kontaktinfo.HentAsync(User);
        if (kontakt == null)
            return Challenge();

        return View(new BehovViewModel { Kontaktpunkt = kontakt.SomTekst() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.KanRegistrereBehov)]
    public async Task<IActionResult> Registrer(BehovViewModel model)
    {
        // Kontaktpunktet hentes alltid fra den innloggede brukeren, aldri fra skjemaet
        var kontakt = await _kontaktinfo.HentAsync(User);
        if (kontakt == null)
            return Challenge();
        model.Kontaktpunkt = kontakt.SomTekst();

        if (!ModelState.IsValid)
            return View(model);

        // Fra view-modell til databaseentitet
        var behov = new Behov
        {
            TypeBehov = model.TypeBehov,
            Beskrivelse = model.Beskrivelse,
            GeografiskOmrade = model.GeografiskOmrade,
            Prioritet = model.Prioritet,
            Kontaktpunkt = model.Kontaktpunkt,
            Status = model.Status,
            Latitude = model.Latitude!.Value,
            Longitude = model.Longitude!.Value,
            Radius = model.Radius,
            OpprettetAvId = InnloggetBrukerId()
        };

        await _repo.AddAsync(behov);
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Koordinator)]
    public async Task<IActionResult> Oversikt()
    {
        var behovene = await _repo.GetAllAsync();
        return View(TilVisning(behovene));
    }

    // Behovene den innloggede brukeren selv har registrert. Bruker samme visning som oversikten.
    [HttpGet]
    [Authorize(Roles = RoleNames.KanRegistrereBehov)]
    public async Task<IActionResult> Mine()
    {
        var brukerId = InnloggetBrukerId();
        var behovene = await _repo.GetAllAsync();
        var mine = behovene.Where(b => brukerId != null && b.OpprettetAvId == brukerId);

        ViewData["Overskrift"] = "Mine behov";
        return View("Oversikt", TilVisning(mine));
    }

    // Fra entitet tilbake til view-modell, slik at oversikten fungerer som før
    private static List<BehovViewModel> TilVisning(IEnumerable<Behov> behovene)
    {
        return behovene.Select(b => new BehovViewModel
        {
            Id = b.Id,
            TypeBehov = b.TypeBehov,
            Beskrivelse = b.Beskrivelse,
            GeografiskOmrade = b.GeografiskOmrade,
            OpprettetTidspunkt = b.OpprettetTidspunkt,
            Prioritet = b.Prioritet,
            Kontaktpunkt = b.Kontaktpunkt,
            Status = b.Status,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            Radius = b.Radius
        }).ToList();
    }

    // Id-en til brukeren som er logget inn. Er null hvis ingen er logget inn (for eksempel i enhetstester).
    private string? InnloggetBrukerId()
    {
        return User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
