using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.Models;

namespace gruppe11_beredskap.Controllers;

//håndetere inmeldelser av kriser/behov
public class BehovController : Controller
{
    // Midlertidig lagring blir erstattet med database senere i semesteret
    private static readonly List<BehovViewModel> _behovListe = new();

    [HttpGet]
    public IActionResult Registrer()
    {
        return View(new BehovViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registrer(BehovViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _behovListe.Add(model);
        return RedirectToAction(nameof(Oversikt));
    }

    [HttpGet]
    public IActionResult Oversikt()
    {
        return View(_behovListe);
    }
}