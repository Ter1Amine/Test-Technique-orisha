using Moq;
using Reception.Application.Services;
using Reception.Domain.Interfaces;
using Reception.Domain.Models;

namespace Reception.Tests.Services;

public class ProductReceiptServiceTests
{
    private readonly Mock<ICommandRepository> _repository = new();
    private readonly ProductReceiptService _service;

    public ProductReceiptServiceTests() => _service = new ProductReceiptService(_repository.Object);

    [Theory]
    [InlineData("PRD-1", true, new[] { false, false, false }, "PartiallyReceived", "PartiallyReceived")]
    [InlineData("PRD-3", true, new[] { true, true, false }, "Received", "Received")]
    [InlineData("PRD-1", false, new[] { true, true, true }, "PartiallyReceived", "PartiallyReceived")]
    [InlineData("PRD-3", false, new[] { false, false, true }, "NotReceived", "NotReceived")]
    public async Task UpdateReceipt_UpdatesProductAndRecomputesStatuses(
        string productId, bool isReceived, bool[] initialStates, string expectedCartonStatus, string expectedCommandStatus)
    {
        var command = TestData.BuildCommand("CMD-1", initialStates);
        _repository.Setup(r => r.GetCommandById("CMD-1")).ReturnsAsync(command);

        var result = await _service.UpdateReceipt("CMD-1", productId, isReceived);

        Assert.NotNull(result);
        var carton = result.Palettes.SelectMany(p => p.Cartons).Single(c => c.Products.Any(p => p.RefId == productId));
        Assert.Equal(isReceived, carton.Products.Single(p => p.RefId == productId).IsReceived);
        Assert.Equal(expectedCartonStatus, carton.Status);
        Assert.Equal(expectedCommandStatus, result.Status);
        _repository.Verify(r => r.SaveChangeAsync(), Times.Once);
    }

    [Theory]
    [InlineData("UNKNOWN", "PRD-1")]
    [InlineData("CMD-1", "UNKNOWN")]
    public async Task UpdateReceipt_ReturnsNull_WhenCommandOrProductDoesNotExist(string commandId, string productId)
    {
        _repository.Setup(r => r.GetCommandById("CMD-1")).ReturnsAsync(TestData.BuildCommand("CMD-1"));
        _repository.Setup(r => r.GetCommandById("UNKNOWN")).ReturnsAsync((Command)null!);

        var result = await _service.UpdateReceipt(commandId, productId, true);

        Assert.Null(result);
        _repository.Verify(r => r.SaveChangeAsync(), Times.Never);
    }
}
