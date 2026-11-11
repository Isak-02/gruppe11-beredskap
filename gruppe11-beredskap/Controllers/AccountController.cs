using System.Text;
using System.Text.Encodings.Web;
using gruppe11_beredskap.Models;
using gruppe11_beredskap.Models.Authorization;
using gruppe11_beredskap.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace gruppe11_beredskap.Controllers;

// Innlogging, utlogging, registrering og glemt passord (fra Identity-labben).
// "Logg inn" og "Opprett bruker" bruker det samme kortet (Views/Shared/_Innlogging.cshtml).
public class AccountController : Controller
{
    private readonly UserManager<Bruker> _userManager;
    private readonly SignInManager<Bruker> _signInManager;
    private readonly IEmailSender _emailSender;

    public AccountController(
        UserManager<Bruker> userManager,
        SignInManager<Bruker> signInManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }

    // Hit sendes man automatisk når man åpner en side som krever innlogging
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new InnloggingViewModel { ReturnUrl = returnUrl });
    }

    // Feltene i skjemaet heter LoggInn.Epost og LoggInn.Passord, derfor Prefix = "LoggInn"
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([Bind(Prefix = "LoggInn")] LoggInnViewModel model, string? returnUrl = null)
    {
        var visning = new InnloggingViewModel { LoggInn = model, ReturnUrl = returnUrl };

        if (!ModelState.IsValid)
            return View(visning);

        var bruker = await _userManager.FindByEmailAsync(model.Epost);
        if (bruker != null)
        {
            var resultat = await _signInManager.PasswordSignInAsync(
                bruker, model.Passord, isPersistent: false, lockoutOnFailure: true);

            if (resultat.Succeeded)
                return TilLokalAdresse(returnUrl);

            if (resultat.IsLockedOut)
            {
                visning.LoggInnFeil = "Kontoen er midlertidig låst etter for mange mislykkede forsøk. Prøv igjen senere.";
                return View(visning);
            }
        }

        // Samme melding enten e-posten eller passordet er feil, så ingen kan finne ut hvem som har bruker
        visning.LoggInnFeil = "Feil e-postadresse eller passord.";
        return View(visning);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        return View(new InnloggingViewModel { VisRegistrering = true, ReturnUrl = returnUrl });
    }

    // Den som oppretter bruker selv, får alltid rollen RessursInnmelder.
    // KriseInnmelder og Koordinator opprettes av oss, så fremmede ikke kan gi seg selv mer tilgang.
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([Bind(Prefix = "Registrer")] RegistrerBrukerViewModel model, string? returnUrl = null)
    {
        var visning = new InnloggingViewModel { Registrer = model, VisRegistrering = true, ReturnUrl = returnUrl };

        if (!ModelState.IsValid)
            return View(visning);

        var bruker = new Bruker
        {
            UserName = model.Epost,
            Email = model.Epost,
            EmailConfirmed = true,
            Navn = model.Navn,
            PhoneNumber = string.IsNullOrWhiteSpace(model.Telefon) ? null : model.Telefon
        };

        var resultat = await _userManager.CreateAsync(bruker, model.Passord);
        if (!resultat.Succeeded)
        {
            visning.RegistrerFeil = resultat.Errors.Select(e => e.Description).ToList();
            return View(visning);
        }

        await _userManager.AddToRoleAsync(bruker, RoleNames.RessursInnmelder);
        await _signInManager.SignInAsync(bruker, isPersistent: false);
        return TilLokalAdresse(returnUrl);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new GlemtPassordViewModel());
    }

    // Lenken for å lage nytt passord skrives i Run-vinduet (ConsoleEmailSender), siden vi ikke sender ekte e-post
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(GlemtPassordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var bruker = await _userManager.FindByEmailAsync(model.Epost);
        if (bruker != null && await _userManager.IsEmailConfirmedAsync(bruker))
        {
            var kode = await _userManager.GeneratePasswordResetTokenAsync(bruker);
            var kodetKode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(kode));
            var lenke = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new { code = kodetKode, email = bruker.Email },
                    Request.Scheme)
                ?? throw new InvalidOperationException("Kunne ikke lage lenken for nytt passord.");

            await _emailSender.SendEmailAsync(
                bruker.Email!,
                "Lag nytt passord",
                $"Lag nytt passord ved å <a href='{HtmlEncoder.Default.Encode(lenke)}'>klikke her</a>.");
        }

        // Samme svar enten brukeren finnes eller ikke, så ingen kan finne ut hvem som har bruker
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(string? code = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(email))
            return BadRequest("Lenken for å lage nytt passord mangler kode eller e-postadresse.");

        return View(new TilbakestillPassordViewModel { Kode = code, Epost = email });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(TilbakestillPassordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var bruker = await _userManager.FindByEmailAsync(model.Epost);
        if (bruker == null)
            return RedirectToAction(nameof(ResetPasswordConfirmation));

        string kode;
        try
        {
            kode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Kode));
        }
        catch (FormatException)
        {
            ModelState.AddModelError(string.Empty, "Lenken for å lage nytt passord er ugyldig.");
            return View(model);
        }

        var resultat = await _userManager.ResetPasswordAsync(bruker, kode, model.Passord);
        if (resultat.Succeeded)
            return RedirectToAction(nameof(ResetPasswordConfirmation));

        foreach (var feil in resultat.Errors)
        {
            ModelState.AddModelError(string.Empty, feil.Description);
        }

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    // Viser innstillinger for den innloggede brukeren
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Innstillinger()
    {
        var bruker = await _userManager.GetUserAsync(User);

        if (bruker == null)
            return Challenge();

        ViewBag.Navn = bruker.Navn;
        ViewBag.Epost = bruker.Email;
        ViewBag.Telefon = bruker.PhoneNumber;

        return View();
    }


    // Sender bare videre til adresser på vår egen side, så lenker ikke kan sende brukeren til en annen nettside
    private IActionResult TilLokalAdresse(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
