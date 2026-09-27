using Moq;
using Reception.Application.Services;
using Reception.Domain.Interfaces;
using Reception.Domain.Models;

namespace Reception.Tests.Services;

public class PaletteReceiptServiceTests
{
    private readonly Mock<ICommandRepository> _repository = new();
    private readonly PaletteReceiptService _service;

    public PaletteReceiptServiceTests() => _service = new PaletteReceiptService(_repository.Object);

    [Theory]
    [InlineData(true, "Received", "PartiallyReceived")]
    [InlineData(false, "NotReceived", "NotReceived")]
    public async Task UpdateReceipt_UpdatesOnlyProductsOfThePalette(
        bool isReceived, string expectedPaletteStatus, string expectedCommandStatus)
    {
        var command = TestData.BuildCommand("CMD-1", !isReceived, !isReceived, false);
        _repository.Setup(r => r.GetCommandById("CMD-1")).ReturnsAsync(command);

        var result = await _service.UpdateReceipt("CMD-1", "PAL-1", isReceived);

        Assert.NotNull(result);
        Assert.Equal(expectedPaletteStatus, result.Palettes.Single(p => p.PaletteId == "PAL-1").Status);
        Assert.Equal(expectedCommandStatus, result.Status);
        Assert.False(command.Palettes.Single(p => p.PaletteId == "PAL-2").Cartons.Single().Products.Single().IsReceived);
        _repository.Verify(r => r.SaveChangeAsync(), Times.Once);
    }

    [Theory]
    [InlineData("UNKNOWN", "PAL-1")]
    [InlineData("CMD-1", "UNKNOWN")]
    public async Task UpdateReceipt_ReturnsNull_WhenCommandOrPaletteDoesNotExist(string commandId, string paletteId)
    {
        _repository.Setup(r => r.GetCommandById("CMD-1")).ReturnsAsync(TestData.BuildCommand("CMD-1"));
        _repository.Setup(r => r.GetCommandById("UNKNOWN")).ReturnsAsync((Command)null!);

        var result = await _service.UpdateReceipt(commandId, paletteId, true);

        Assert.Null(result);
        _repository.Verify(r => r.SaveChangeAsync(), Times.Never);
    }
}
