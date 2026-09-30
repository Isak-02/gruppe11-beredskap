using Microsoft.AspNetCore.Mvc;
using gruppe11_beredskap.Controllers;
using gruppe11_beredskap.Models;

namespace gruppe11_beredskap.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsViewResult()
    {
        var controller = new HomeController();

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void CorrectMap_Get_ReturnsViewResult()
    {
        var controller = new HomeController();

        var result = controller.CorrectMap();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void CorrectMap_Post_WithValidModel_ReturnsCorrectionOverview()
    {
        var controller = new HomeController();

        var model = new PositionModel
        {
            Latitude = "58.1467",
            Longitude = "7.9956",
            Description = "Test position"
        };

        var result = controller.CorrectMap(model);

        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal("CorrectionOverview", viewResult.ViewName);
    }
}