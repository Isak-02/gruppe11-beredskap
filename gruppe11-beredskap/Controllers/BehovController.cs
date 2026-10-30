using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.Controllers;

// Håndterer innmelding og visning av behov. Lagrer i MariaDB via repository.
public class BehovController : Controller
{
    private readonly IBehovRepository _repo;

    public BehovController(IBehovRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult Registrer()
    {
        return View(new BehovViewModel
        {
            Tidspunkt = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrer(BehovViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Fra view-modell til databaseentitet
        var behov = new Behov
        {
            TypeBehov = model.TypeBehov,
            GeografiskOmrade = model.GeografiskOmrade,
            Tidspunkt = model.Tidspunkt,
            Prioritet = model.Prioritet,
            Kontaktpunkt = model.Kontaktpunkt,
            Status = model.Status,
            Latitude = model.Latitude!.Value,
            Longitude = model.Longitude!.Value,
            Radius = model.Radius
        };

        await _repo.AddAsync(behov);
        return RedirectToAction(nameof(Oversikt));
    }

    [HttpGet]
    public async Task<IActionResult> Oversikt()
    {
        var behovene = await _repo.GetAllAsync();

        // Fra entitet tilbake til view-modell, slik at oversikten fungerer som før
        var visning = behovene.Select(b => new BehovViewModel
        {
            Id = b.Id,
            TypeBehov = b.TypeBehov,
            GeografiskOmrade = b.GeografiskOmrade,
            Tidspunkt = b.Tidspunkt,
            Prioritet = b.Prioritet,
            Kontaktpunkt = b.Kontaktpunkt,
            Status = b.Status,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            Radius = b.Radius
        }).ToList();

        return View(visning);
    }
}
