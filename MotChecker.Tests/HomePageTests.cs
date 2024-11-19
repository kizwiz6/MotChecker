using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MotChecker.Services;
using MotChecker.Pages;
using Moq;
using MotChecker.Models;

namespace MotChecker.Tests;

public class HomePageTests : TestContext
{
    private readonly Mock<IVehicleService> _vehicleServiceMock;

    public HomePageTests()
    {
        _vehicleServiceMock = new Mock<IVehicleService>();
        Services.AddScoped<IVehicleService>(_ => _vehicleServiceMock.Object);
    }

    [Fact]
    public void Should_Show_Search_Form_Initially()
    {
        // Arrange & Act
        var cut = RenderComponent<MotChecker.Pages.Home>();

        // Assert
        cut.Find("input#registration").Should().NotBeNull();
        cut.Find("button[type='submit']").Should().NotBeNull();
        cut.Find("h1").TextContent.Should().Contain("MOT History Checker");
    }

    [Fact]
    public void Should_Show_Validation_Message_For_Invalid_Input()
    {
        // Arrange
        var cut = RenderComponent<Home>();

        // Act
        var form = cut.Find("form");
        var input = cut.Find("input#registration");
        input.Change("!@#"); // Invalid input
        form.Submit();

        // Assert
        var validationMessage = cut.Find(".text-red-500");
        validationMessage.TextContent.Should()
            .Contain("Please enter a valid UK registration number");
    }

    [Fact]
    public async Task Should_Display_Vehicle_Details_When_Search_Successful()
    {
        // Arrange
        var vehicleDetails = new VehicleDetails
        {
            Registration = "LB11WXA",
            Make = "Kia",
            Model = "Rio",
            Colour = "Silver",
            MotExpiryDate = DateTime.Now.AddMonths(6),
            MileageAtLastMot = 90000
        };

        _vehicleServiceMock.Setup(x => x.GetVehicleDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync(vehicleDetails);

        var cut = RenderComponent<Home>();

        // Act
        var form = cut.Find("form");
        var input = cut.Find("input#registration");
        input.Change("LB11WXA");
        await form.SubmitAsync();

        // Assert
        cut.FindAll("dd").Count.Should().Be(6); // All vehicle details fields
        cut.Markup.Should().Contain(vehicleDetails.Make);
        cut.Markup.Should().Contain(vehicleDetails.Model);
        cut.Markup.Should().Contain(vehicleDetails.Colour);
        cut.Markup.Should().Contain(vehicleDetails.MileageAtLastMot.ToString("N0"));
    }

    [Fact]
    public async Task Should_Show_Error_Message_When_Service_Fails()
    {
        // Arrange
        _vehicleServiceMock.Setup(x => x.GetVehicleDetailsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("API Error"));

        var cut = RenderComponent<Home>();

        // Act
        var form = cut.Find("form");
        var input = cut.Find("input#registration");
        input.Change("LB11WXA");
        await form.SubmitAsync();

        // Assert
        var errorMessage = cut.Find("[role='alert']");
        errorMessage.Should().NotBeNull();
        errorMessage.TextContent.Should().Contain("Unable to retrieve vehicle details");
    }

    [Fact]
    public async Task Should_Show_Loading_State_During_Search()
    {
        // Arrange
        var tcs = new TaskCompletionSource<VehicleDetails>();
        _vehicleServiceMock.Setup(x => x.GetVehicleDetailsAsync(It.IsAny<string>()))
            .Returns(tcs.Task);

        var cut = RenderComponent<Home>();

        // Act
        var form = cut.Find("form");
        var input = cut.Find("input#registration");
        input.Change("LB11WXA");
        var submitTask = form.SubmitAsync();

        // Assert
        var button = cut.Find("button[type='submit']");
        button.ClassList.Should().Contain("opacity-50");
        button.ClassList.Should().Contain("cursor-not-allowed");
        button.TextContent.Should().Contain("Searching");

        // Complete the search
        tcs.SetResult(new VehicleDetails());
        await submitTask;
    }

    [Fact]
    public void Should_Convert_Registration_To_Uppercase()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        var input = cut.Find("input#registration");

        // Act
        // Trigger both Change and Input events
        var registration = "lb11wxa";
        input.Change(registration);
        input.Input(registration);

        // Assert
        input.GetAttribute("value").Should().Be("LB11WXA");

        // Submit and verify
        var form = cut.Find("form");
        form.Submit();

        _vehicleServiceMock.Verify(x =>
            x.GetVehicleDetailsAsync(It.Is<string>(s => s == "LB11WXA")),
            Times.Once);
    }
}