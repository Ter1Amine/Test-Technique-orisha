using Moq;
using Reception.Application.Services;
using Reception.Core.Dtos;
using Reception.Domain.Interfaces;
using Reception.Domain.Models;

namespace Reception.Tests.Services;

public class CommandServiceTests
{
    private readonly Mock<ICommandRepository> _repository = new();
    private readonly CommandService _service;

    public CommandServiceTests()
    {
        _repository
            .Setup(r => r.GetExistingIds(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<string>>()))
            .ReturnsAsync(Array.Empty<string>());
        _service = new CommandService(_repository.Object);
    }

    [Fact]
    public async Task GetCommandById_ReturnsNull_WhenCommandDoesNotExist()
    {
        _repository.Setup(r => r.GetCommandById("UNKNOWN")).ReturnsAsync((Command)null!);

        var result = await _service.GetCommandById("UNKNOWN");

        Assert.Null(result);
    }

    [Theory]
    [InlineData(new[] { false, false, false }, "NotReceived", "0/3")]
    [InlineData(new[] { true, false, false }, "PartiallyReceived", "1/3")]
    [InlineData(new[] { true, true, false }, "PartiallyReceived", "2/3")]
    [InlineData(new[] { true, true, true }, "Received", "3/3")]
    public async Task GetCommandById_ComputesStatusAndReceivedPercent(
        bool[] receivedStates, string expectedStatus, string expectedPercent)
    {
        _repository.Setup(r => r.GetCommandById("CMD-1"))
            .ReturnsAsync(TestData.BuildCommand("CMD-1", receivedStates));

        var result = await _service.GetCommandById("CMD-1");

        Assert.NotNull(result);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedPercent, result.ReceivedPercent);
    }

    [Fact]
    public async Task GetAllCommands_ReturnsEveryCommand()
    {
        _repository.Setup(r => r.GetAllCommands())
            .ReturnsAsync([TestData.BuildCommand("CMD-1"), TestData.BuildCommand("CMD-2")]);

        var result = await _service.GetAllCommands();

        Assert.Equal(["CMD-1", "CMD-2"], result.Select(c => c.CommandId));
    }

    [Theory]
    [InlineData(true, "Received")]
    [InlineData(false, "NotReceived")]
    public async Task UpdateReceipt_UpdatesEveryProductOfTheCommand(bool isReceived, string expectedStatus)
    {
        var command = TestData.BuildCommand("CMD-1", !isReceived, !isReceived, !isReceived);
        _repository.Setup(r => r.GetCommandById("CMD-1")).ReturnsAsync(command);

        var result = await _service.UpdateReceipt("CMD-1", isReceived);

        Assert.NotNull(result);
        Assert.Equal(expectedStatus, result.Status);
        Assert.All(TestData.AllProducts(command), p => Assert.Equal(isReceived, p.IsReceived));
        _repository.Verify(r => r.SaveChangeAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateReceipt_ReturnsNull_WhenCommandDoesNotExist()
    {
        _repository.Setup(r => r.GetCommandById("UNKNOWN")).ReturnsAsync((Command)null!);

        var result = await _service.UpdateReceipt("UNKNOWN", true);

        Assert.Null(result);
        _repository.Verify(r => r.SaveChangeAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateCommand_SavesCommandAsNotReceived_WhenRequestIsValid()
    {
        Command? saved = null;
        _repository.Setup(r => r.AddCommand(It.IsAny<Command>()))
            .Callback<Command>(c => saved = c)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateCommand(ValidRequest());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Command);
        Assert.Equal("NotReceived", result.Command.Status);
        Assert.Equal("0/2", result.Command.ReceivedPercent);
        Assert.NotNull(saved);
        Assert.All(TestData.AllProducts(saved), p => Assert.False(p.IsReceived));
        _repository.Verify(r => r.AddCommand(It.IsAny<Command>()), Times.Once);
    }

    public static TheoryData<CreateCommandRequest, string> InvalidRequests => new()
    {
        { ValidRequest() with { CommandId = " " }, "L'identifiant de la commande est obligatoire." },
        { ValidRequest() with { Palettes = [] }, "La commande doit contenir au moins une palette." },
        {
            ValidRequest() with { Palettes = [new CreatePaletteRequest("PAL-1", [])] },
            "La palette PAL-1 doit contenir au moins un carton."
        },
        {
            ValidRequest() with { Palettes = [new CreatePaletteRequest("PAL-1", [new CreateCartonRequest("CAR-1", [])])] },
            "Le carton CAR-1 doit contenir au moins un produit."
        },
        {
            WithProduct(new CreateProductRequest("PRD-1", "", "Blue", "M", 1)),
            "Le produit PRD-1 doit avoir un nom, une couleur et une taille."
        },
        {
            WithProduct(new CreateProductRequest("PRD-1", "Shirt", "Blue", "M", 0)),
            "La quantité du produit PRD-1 doit être supérieure à 0."
        },
        {
            WithProduct(new CreateProductRequest("CAR-1", "Shirt", "Blue", "M", 1)),
            "L'identifiant CAR-1 est utilisé plusieurs fois."
        }
    };

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async Task CreateCommand_ReturnsErrors_WhenRequestIsInvalid(CreateCommandRequest request, string expectedError)
    {
        var result = await _service.CreateCommand(request);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Command);
        Assert.Contains(expectedError, result.Errors);
        _repository.Verify(r => r.AddCommand(It.IsAny<Command>()), Times.Never);
    }

    [Fact]
    public async Task CreateCommand_ReturnsErrors_WhenIdsAlreadyExist()
    {
        _repository
            .Setup(r => r.GetExistingIds(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<string>>()))
            .ReturnsAsync(["Commande CMD-NEW", "Produit PRD-A"]);

        var result = await _service.CreateCommand(ValidRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(["Commande CMD-NEW existe déjà.", "Produit PRD-A existe déjà."], result.Errors);
        _repository.Verify(r => r.AddCommand(It.IsAny<Command>()), Times.Never);
    }

    private static CreateCommandRequest ValidRequest() => new(
        "CMD-NEW",
        [
            new CreatePaletteRequest("PAL-A",
            [
                new CreateCartonRequest("CAR-A",
                [
                    new CreateProductRequest("PRD-A", "Shirt", "Blue", "M", 2),
                    new CreateProductRequest("PRD-B", "Pants", "Black", "L", 1)
                ])
            ])
        ]);

    private static CreateCommandRequest WithProduct(CreateProductRequest product) => new(
        "CMD-NEW",
        [new CreatePaletteRequest("PAL-1", [new CreateCartonRequest("CAR-1", [product])])]);
}
