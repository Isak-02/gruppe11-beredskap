using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Authorization;

namespace gruppe11_beredskap.Controllers;

public class HomeController : Controller
{
    // Forsiden vises bare for den som ikke er logget inn.
    // Innloggede sendes videre til siden som passer rollen deres.
    public IActionResult Index()
    {
        if (User?.IsInRole(RoleNames.Koordinator) == true)
            return RedirectToAction("Index", "Matching");
        if (User?.IsInRole(RoleNames.KriseInnmelder) == true)
            return RedirectToAction("Mine", "Behov");
        if (User?.IsInRole(RoleNames.RessursInnmelder) == true)
            return RedirectToAction("Mine", "Ressurs");

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
