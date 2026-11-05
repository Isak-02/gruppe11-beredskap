using Microsoft.AspNetCore.Mvc;

namespace gruppe11_beredskap.Controllers;

public class KontoController : Controller
{
    [HttpGet]
    public IActionResult Innstillinger()
    {
        return View();
    }
    [HttpGet]
    public IActionResult MinProfil()
    {
        return View();
    }
}