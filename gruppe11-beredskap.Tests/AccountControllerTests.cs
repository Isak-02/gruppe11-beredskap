
using System.Reflection;
using System.Security.Claims;
using gruppe11_beredskap.Controllers;
using gruppe11_beredskap.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace gruppe11_beredskap.Tests;

public class AccountControllerTests
{
    private readonly Mock<UserManager<Bruker>> _userManagerMock;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        var userStore = new Mock<IUserStore<Bruker>>();

        _userManagerMock = new Mock<UserManager<Bruker>>(
            userStore.Object,
            null!,
            null!,
            Array.Empty<IUserValidator<Bruker>>(),
            Array.Empty<IPasswordValidator<Bruker>>(),
            null!,
            null!,
            null!,
            null!);

        _controller = new AccountController(
            _userManagerMock.Object,
            null!,
            null!);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public void Innstillinger_KreverInnlogging()
    {
        var metode = typeof(AccountController)
            .GetMethod(nameof(AccountController.Innstillinger));

        Assert.NotNull(metode);
        Assert.NotNull(
            metode.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public async Task Innstillinger_HenterRiktigBrukerinformasjon()
    {
        var bruker = new Bruker
        {
            Navn = "Ola Nordmann",
            Email = "ola@example.com",
            PhoneNumber = "12345678"
        };

        _userManagerMock
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(bruker);

        var resultat = await _controller.Innstillinger();

        Assert.IsType<ViewResult>(resultat);
        Assert.Equal("Ola Nordmann", _controller.ViewBag.Navn);
        Assert.Equal("ola@example.com", _controller.ViewBag.Epost);
        Assert.Equal("12345678", _controller.ViewBag.Telefon);
    }

    [Fact]
    public async Task Innstillinger_UtenBruker_ReturnererChallenge()
    {
        _userManagerMock
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((Bruker?)null);

        var resultat = await _controller.Innstillinger();

        Assert.IsType<ChallengeResult>(resultat);
    }
}
