using Moq;
using Reception.Application.Services;
using Reception.Domain.Interfaces;
using Reception.Domain.Models;

namespace Reception.Tests.Services;

public class CartonReceiptServiceTests
{
    private readonly Mock<ICommandRepository> _repository = new();
    private readonly CartonReceiptService _service;

    public CartonReceiptServiceTests() => _service = new CartonReceiptService(_repository.Object);

    [Theory]
    [InlineData(true, "Received", "PartiallyReceived")]
    [InlineData(false, "NotReceived", "NotReceived")]
    public async Task UpdateReceipt_UpdatesOnlyProductsOfTheCarton(
        bool isReceived, string expectedCartonStatus, string expectedCommandStatus)
    {
        var command = TestData.BuildCommand("CMD-1", !isReceived, !isReceived, false);
        _repository.Setup(r => r.GetCommandById("CMD-1")).ReturnsAsync(command);

        var result = await _service.UpdateReceipt("CMD-1", "CAR-1", isReceived);

        Assert.NotNull(result);
        var carton = result.Palettes.SelectMany(p => p.Cartons).Single(c => c.CartonId == "CAR-1");
        Assert.Equal(expectedCartonStatus, carton.Status);
        Assert.All(carton.Products, p => Assert.Equal(isReceived, p.IsReceived));
        Assert.Equal(expectedCommandStatus, result.Status);
        Assert.False(TestData.AllProducts(command).Single(p => p.RefId == "PRD-3").IsReceived);
        _repository.Verify(r => r.SaveChangeAsync(), Times.Once);
    }

    [Theory]
    [InlineData("UNKNOWN", "CAR-1")]
    [InlineData("CMD-1", "UNKNOWN")]
    public async Task UpdateReceipt_ReturnsNull_WhenCommandOrCartonDoesNotExist(string commandId, string cartonId)
    {
        _repository.Setup(r => r.GetCommandById("CMD-1")).ReturnsAsync(TestData.BuildCommand("CMD-1"));
        _repository.Setup(r => r.GetCommandById("UNKNOWN")).ReturnsAsync((Command)null!);

        var result = await _service.UpdateReceipt(commandId, cartonId, true);

        Assert.Null(result);
        _repository.Verify(r => r.SaveChangeAsync(), Times.Never);
    }
}
