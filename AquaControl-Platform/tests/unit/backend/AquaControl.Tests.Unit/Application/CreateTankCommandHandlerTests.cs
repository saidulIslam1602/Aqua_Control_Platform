using FluentAssertions;
using Moq;
using AquaControl.Application.Features.Tanks.Commands.CreateTank;
using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.ValueObjects;
using AquaControl.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AquaControl.Tests.Unit.Application;

public class CreateTankCommandHandlerTests
{
    private readonly Mock<ITankRepository> _tankRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateTankCommandHandler>> _loggerMock;
    private readonly CreateTankCommandHandler _handler;

    public CreateTankCommandHandlerTests()
    {
        _tankRepositoryMock = new Mock<ITankRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateTankCommandHandler>>();
        _handler = new CreateTankCommandHandler(
            _tankRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTankSuccessfully()
    {
        // Arrange
        var command = new CreateTankCommand(
            "Test Tank",
            1000,
            "L",
            "Building A",
            "Room 1",
            null,
            null,
            null,
            TankType.Freshwater);

        _tankRepositoryMock
            .Setup(x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tank?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _tankRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Tank>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingTankName_ShouldReturnConflictError()
    {
        // Arrange
        var command = new CreateTankCommand(
            "Existing Tank",
            1000,
            "L",
            "Building A",
            "Room 1",
            null,
            null,
            null,
            TankType.Freshwater);

        var existingTank = CreateValidTank();
        _tankRepositoryMock
            .Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTank);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Tank.NameAlreadyExists");

        _tankRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Tank>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldReturnFailureError()
    {
        // Arrange
        var command = new CreateTankCommand(
            "Test Tank",
            1000,
            "L",
            "Building A",
            "Room 1",
            null,
            null,
            null,
            TankType.Freshwater);

        _tankRepositoryMock
            .Setup(x => x.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tank?)null);

        _tankRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Tank>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Failure);
        result.Error.Code.Should().Be("Tank.CreationFailed");
    }

    private static Tank CreateValidTank()
    {
        var capacity = TankCapacity.Create(1000, "L");
        var location = Location.Create("Building A", "Room 1");
        return Tank.Create("Test Tank", capacity, location, TankType.Freshwater);
    }
}

