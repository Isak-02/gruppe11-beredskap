using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.Controllers;

// Håndterer innmelding og visning av ressurser. Lagrer i MariaDB via repository.
public class RessursController : Controller
{
    private readonly IRessursRepository _repo;

    public RessursController(IRessursRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult Registrer()
    {
        return View(new RessursViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrer(RessursViewModel model)
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

        if (!ModelState.IsValid)
            return View(model);

        // Fra view-modell til databaseentitet
        var ressurs = new Ressurs
        {
            TypeRessurs = model.TypeRessurs,
            AnnenType = model.AnnenType,
            Drivstofftype = model.Drivstofftype,
            Lastekapasitet = model.Lastekapasitet,
            GeografiskOmrade = model.GeografiskOmrade,
            Tidspunkt = model.Tidspunkt,
            Kontaktpunkt = model.Kontaktpunkt,
            Status = model.Status,
            Latitude = model.Latitude!.Value,
            Longitude = model.Longitude!.Value
        };

        await _repo.AddAsync(ressurs);
        return RedirectToAction(nameof(Oversikt));
    }

    [HttpGet]
    public async Task<IActionResult> Oversikt()
    {
        var ressurser = await _repo.GetAllAsync();

        // Fra entitet tilbake til view-modell, slik at oversikten fungerer som før
        var visning = ressurser.Select(r => new RessursViewModel
        {
            Id = r.Id,
            TypeRessurs = r.TypeRessurs,
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

        return View(visning);
    }
}
