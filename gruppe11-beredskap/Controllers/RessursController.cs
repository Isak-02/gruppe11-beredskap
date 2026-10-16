using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.Models;

namespace gruppe11_beredskap.Controllers;

//håndtere innmeldinger av ressurser 
public class RessursController : Controller
{
    // Midlertidig lagring blir erstattet med database senere i semesteret
    private static readonly List<RessursViewModel> _ressursListe = new();

    [HttpGet]
    public IActionResult Registrer()
    {
        return View(new RessursViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registrer(RessursViewModel model)
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

        _ressursListe.Add(model);
        return RedirectToAction(nameof(Oversikt));
    }

    [HttpGet]
    public IActionResult Oversikt()
    {
        return View(_ressursListe);
    }
}