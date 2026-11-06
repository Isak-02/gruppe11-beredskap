using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Authorization;
using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.Controllers;

// Håndterer innmelding og visning av ressurser. Lagrer i MariaDB via repository.
// Ressurs-innmeldere og koordinatorer kan registrere ressurser og se sine egne. Bare koordinatorer ser alle.
public class RessursController : Controller
{
    private readonly IRessursRepository _repo;

    public RessursController(IRessursRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.KanRegistrereRessurs)]
    public IActionResult Registrer()
    {
        return View(new RessursViewModel
        {
            Tidspunkt = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.KanRegistrereRessurs)]
    public async Task<IActionResult> Registrer(RessursViewModel model)
    {
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
            Tidspunkt = model.Tidspunkt,
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

        LeggTilFeltfeil(model);
        if (!ModelState.IsValid)
            return View("Registrer", model);

        ressurs.TypeRessurs = model.TypeRessurs;
        ressurs.Beskrivelse = model.Beskrivelse;
        ressurs.AnnenType = model.AnnenType;
        ressurs.Drivstofftype = model.Drivstofftype;
        ressurs.Lastekapasitet = model.Lastekapasitet;
        ressurs.GeografiskOmrade = model.GeografiskOmrade;
        ressurs.Tidspunkt = model.Tidspunkt;
        ressurs.Kontaktpunkt = model.Kontaktpunkt;
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

    // Tilgjengelige ressurser som kan filtreres på fylke i kartet og skrives ut.
    // Selve fylkesfilteret skjer i visningen, ut fra posisjonen til hver ressurs.
    [HttpGet]
    [Authorize(Roles = RoleNames.Koordinator)]
    public async Task<IActionResult> PerFylke()
    {
        var ressurser = await _repo.GetAllAsync();
        var tilgjengelige = ressurser.Where(r => r.Status == RessursStatus.Tilgjengelig);

        return View(TilVisning(tilgjengelige));
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
            Tidspunkt = r.Tidspunkt,
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
