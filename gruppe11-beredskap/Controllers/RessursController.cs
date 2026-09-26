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