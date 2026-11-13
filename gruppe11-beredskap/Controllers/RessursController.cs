using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Authorization;
using gruppe11_beredskap.Models.Entities;
using gruppe11_beredskap.Services;

namespace gruppe11_beredskap.Controllers;

// Håndterer innmelding og visning av ressurser. Lagrer i MariaDB via repository.
// Ressurs-innmeldere og koordinatorer kan registrere ressurser og se sine egne. Bare koordinatorer ser alle.
public class RessursController : Controller
{
    private readonly IRessursRepository _repo;
    private readonly IKontaktinfoTjeneste _kontaktinfo;

    public RessursController(IRessursRepository repo, IKontaktinfoTjeneste kontaktinfo)
    {
        _repo = repo;
        _kontaktinfo = kontaktinfo;
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.KanRegistrereRessurs)]
    public async Task<IActionResult> Registrer()
    {
        var kontakt = await _kontaktinfo.HentAsync(User);
        if (kontakt == null)
            return Challenge();

        return View(new RessursViewModel { Kontaktpunkt = kontakt.SomTekst() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.KanRegistrereRessurs)]
    public async Task<IActionResult> Registrer(RessursViewModel model)
    {
        // Kontaktpunktet hentes alltid fra den innloggede brukeren, aldri fra skjemaet
        var kontakt = await _kontaktinfo.HentAsync(User);
        if (kontakt == null)
            return Challenge();
        model.Kontaktpunkt = kontakt.SomTekst();

        LeggTilFeltfeil(model);
        if (!ModelState.IsValid)
            return View(model);

        // Fra view-modell til databaseentitet
        var ressurs = new Ressurs
        {
            TypeRessurs = model.TypeRessurs,
            Beskrivelse = model.Beskrivelse,
            AnnenType = model.AnnenType,
            Drivstofftype = model.Drivstofftype,
            Lastekapasitet = model.Lastekapasitet,
            GeografiskOmrade = model.GeografiskOmrade,
            Kontaktpunkt = model.Kontaktpunkt,
            Status = model.Status,
            Latitude = model.Latitude!.Value,
            Longitude = model.Longitude!.Value,
            OpprettetAvId = InnloggetBrukerId()
        };

        await _repo.AddAsync(ressurs);
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.KanRegistrereRessurs)]
    public async Task<IActionResult> Endre(int id)
    {
        var ressurs = await _repo.GetByIdAsync(id);
        if (ressurs == null)
            return NotFound();
        if (!EierEllerKoordinator(ressurs))
            return Forbid();

        return View("Registrer", TilVisning(new[] { ressurs }).Single());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.KanRegistrereRessurs)]
    public async Task<IActionResult> Endre(RessursViewModel model)
    {
        var ressurs = await _repo.GetByIdAsync(model.Id);
        if (ressurs == null)
            return NotFound();
        if (!EierEllerKoordinator(ressurs))
            return Forbid();

        // Kontaktpunktet endres ikke. Det tilhører den som registrerte ressursen,
        // også når en koordinator endrer den.
        model.Kontaktpunkt = ressurs.Kontaktpunkt;

        LeggTilFeltfeil(model);
        if (!ModelState.IsValid)
            return View("Registrer", model);

        ressurs.TypeRessurs = model.TypeRessurs;
        ressurs.Beskrivelse = model.Beskrivelse;
        ressurs.AnnenType = model.AnnenType;
        ressurs.Drivstofftype = model.Drivstofftype;
        ressurs.Lastekapasitet = model.Lastekapasitet;
        ressurs.GeografiskOmrade = model.GeografiskOmrade;
        ressurs.Latitude = model.Latitude!.Value;
        ressurs.Longitude = model.Longitude!.Value;

        await _repo.UpdateAsync(ressurs);
        return RedirectToAction(nameof(Mine));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.KanRegistrereRessurs)]
    public async Task<IActionResult> Slett(int id)
    {
        var ressurs = await _repo.GetByIdAsync(id);
        if (ressurs == null)
            return NotFound();
        if (!EierEllerKoordinator(ressurs))
            return Forbid();

        await _repo.DeleteAsync(id);
        return RedirectToAction(nameof(Mine));
    }

    // Setter en tilgjengelig ressurs på pause. Den vises da ikke på Kobling.
    // En tildelt ressurs kan ikke pauses, siden den allerede er i bruk.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.KanRegistrereRessurs)]
    public async Task<IActionResult> Pause(int id)
    {
        var ressurs = await _repo.GetByIdAsync(id);
        if (ressurs == null)
            return NotFound();
        if (!EierEllerKoordinator(ressurs))
            return Forbid();

        if (ressurs.Status == RessursStatus.Tilgjengelig)
            await _repo.UpdateStatusAsync(id, RessursStatus.Pauset);

        return RedirectToAction(nameof(Mine));
    }

    // Gjør en pauset ressurs tilgjengelig igjen
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.KanRegistrereRessurs)]
    public async Task<IActionResult> Gjenoppta(int id)
    {
        var ressurs = await _repo.GetByIdAsync(id);
        if (ressurs == null)
            return NotFound();
        if (!EierEllerKoordinator(ressurs))
            return Forbid();

        if (ressurs.Status == RessursStatus.Pauset)
            await _repo.UpdateStatusAsync(id, RessursStatus.Tilgjengelig);

        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Koordinator)]
    public async Task<IActionResult> Oversikt()
    {
        var ressurser = await _repo.GetAllAsync();
        return View(TilVisning(ressurser));
    }

    // Ressursene den innloggede brukeren selv har registrert. Bruker samme visning som oversikten.
    [HttpGet]
    [Authorize(Roles = RoleNames.KanRegistrereRessurs)]
    public async Task<IActionResult> Mine()
    {
        var brukerId = InnloggetBrukerId();
        var ressurser = await _repo.GetAllAsync();
        var mine = ressurser.Where(r => brukerId != null && r.OpprettetAvId == brukerId);

        ViewData["Overskrift"] = "Mine ressurser";
        return View("Oversikt", TilVisning(mine));
    }

    // Siden er slått sammen med Kobling. Adressen sendes videre, så gamle lenker fortsatt virker.
    [HttpGet]
    [Authorize(Roles = RoleNames.Koordinator)]
    public IActionResult PerFylke()
    {
        return RedirectToAction("Index", "Matching");
    }

    // Fra entitet tilbake til view-modell, slik at oversikten fungerer som før
    private static List<RessursViewModel> TilVisning(IEnumerable<Ressurs> ressurser)
    {
        return ressurser.Select(r => new RessursViewModel
        {
            Id = r.Id,
            TypeRessurs = r.TypeRessurs,
            Beskrivelse = r.Beskrivelse,
            AnnenType = r.AnnenType,
            Drivstofftype = r.Drivstofftype,
            Lastekapasitet = r.Lastekapasitet,
            GeografiskOmrade = r.GeografiskOmrade,
            OpprettetTidspunkt = r.OpprettetTidspunkt,
            Kontaktpunkt = r.Kontaktpunkt,
            Status = r.Status,
            Latitude = r.Latitude,
            Longitude = r.Longitude
        }).ToList();
    }

    private void LeggTilFeltfeil(RessursViewModel model)
    {
        var kjøretøyOgMaskiner = new[]
        {
            "Drone og droneoperatør",
            "ATV",
            "Traktor",
            "Gravemaskin",
            "Hjullaster",
            "Skogsmaskin",
            "Snøskuter",
            "Lastebil",
            "Aggregat",
            "Båt"
        };

        if (kjøretøyOgMaskiner.Contains(model.TypeRessurs) &&
            string.IsNullOrWhiteSpace(model.Drivstofftype))
        {
            ModelState.AddModelError(
                nameof(model.Drivstofftype),
                "Du må velge drivstofftype for denne ressursen");
        }

        var ressurserMedLastekapasitet = new[]
        {
            "Henger",
            "Traktor",
            "Gravemaskin",
            "Hjullaster",
            "Skogsmaskin",
            "Lastebil"
        };

        if (ressurserMedLastekapasitet.Contains(model.TypeRessurs) &&
            (!model.Lastekapasitet.HasValue || model.Lastekapasitet <= 0))
        {
            ModelState.AddModelError(
                nameof(model.Lastekapasitet),
                "Du må skrive inn en lastekapasitet som er større enn 0");
        }
    }

    private bool EierEllerKoordinator(Ressurs ressurs)
    {
        if (User?.IsInRole(RoleNames.Koordinator) == true)
            return true;

        var brukerId = InnloggetBrukerId();
        return brukerId != null && ressurs.OpprettetAvId == brukerId;
    }

    // Id-en til brukeren som er logget inn. Er null hvis ingen er logget inn (for eksempel i enhetstester).
    private string? InnloggetBrukerId()
    {
        return User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
