using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.CreateEquipment;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class CreateEquipmentCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidName_CreatesAndSaves()
    {
        var repository = new Mock<IEquipmentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateEquipmentCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new CreateEquipmentCommand("TV", "55-inch flatscreen"), CancellationToken.None);

        result.Equipment.ShouldNotBeNull();
        result.Equipment.Name.ShouldBe("TV");
        result.Equipment.Description.ShouldBe("55-inch flatscreen");
        result.Errors.ShouldBeEmpty();
        repository.Verify(r => r.AddAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BlankName_ReturnsErrorAndDoesNotSave()
    {
        var repository = new Mock<IEquipmentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateEquipmentCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new CreateEquipmentCommand("   ", null), CancellationToken.None);

        result.Equipment.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
