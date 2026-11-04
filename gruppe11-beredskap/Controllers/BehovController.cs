using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Authorization;
using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.Controllers;

// Håndterer innmelding og visning av behov. Lagrer i MariaDB via repository.
// Krise-innmeldere og koordinatorer kan registrere behov og se sine egne. Bare koordinatorer ser alle.
public class BehovController : Controller
{
    private readonly IBehovRepository _repo;

    public BehovController(IBehovRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.KanRegistrereBehov)]
    public IActionResult Registrer()
    {
        return View(new BehovViewModel
        {
            Tidspunkt = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.KanRegistrereBehov)]
    public async Task<IActionResult> Registrer(BehovViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Fra view-modell til databaseentitet
        var behov = new Behov
        {
            TypeBehov = model.TypeBehov,
            Beskrivelse = model.Beskrivelse,
            GeografiskOmrade = model.GeografiskOmrade,
            Tidspunkt = model.Tidspunkt,
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
            Tidspunkt = b.Tidspunkt,
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
