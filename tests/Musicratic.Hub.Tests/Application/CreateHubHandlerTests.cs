using FluentAssertions;
using Moq;
using Musicratic.Hub.Application;
using Musicratic.Hub.Application.Commands.CreateHub;
using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Enums;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Hub.Domain.Services;
using Musicratic.Shared.Application;
using HubEntity = Musicratic.Hub.Domain.Entities.Hub;

namespace Musicratic.Hub.Tests.Application;

public class CreateHubHandlerTests
{
    private readonly Mock<IHubRepository> _hubRepositoryMock;
    private readonly Mock<IHubCodeGenerator> _hubCodeGeneratorMock;
    private readonly Mock<IHubUnitOfWork> _unitOfWorkMock;
    private readonly CreateHubHandler _handler;

    public CreateHubHandlerTests()
    {
        _hubRepositoryMock = new Mock<IHubRepository>();
        _hubCodeGeneratorMock = new Mock<IHubCodeGenerator>();
        _unitOfWorkMock = new Mock<IHubUnitOfWork>();

        _hubCodeGeneratorMock
            .Setup(g => g.Generate(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("MYBAR42");

        _handler = new CreateHubHandler(
            _hubRepositoryMock.Object,
            _hubCodeGeneratorMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnHubId_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateHubCommand(
            "My Bar",
            HubType.Venue,
            Guid.NewGuid(),
            new HubSettings());

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAdd_WhenCommandIsValid()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new CreateHubCommand(
            "My Bar",
            HubType.Venue,
            ownerId,
            new HubSettings());

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _hubRepositoryMock.Verify(
            r => r.Add(It.Is<HubEntity>(h =>
                h.Name == "My Bar" &&
                h.Type == HubType.Venue &&
                h.OwnerId == ownerId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAutoAddOwnerAsSuperOwner_WhenHubCreated()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new CreateHubCommand(
            "My Bar",
            HubType.Venue,
            ownerId,
            new HubSettings());

        HubEntity? capturedHub = null;
        _hubRepositoryMock
            .Setup(r => r.Add(It.IsAny<HubEntity>(), It.IsAny<CancellationToken>()))
            .Callback<HubEntity, CancellationToken>((h, _) => capturedHub = h)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedHub.Should().NotBeNull();
        capturedHub!.Members.Should().ContainSingle()
            .Which.Should().Match<HubMember>(m =>
                m.UserId == ownerId && m.Role == HubMemberRole.SuperOwner);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenHubIsAdded()
    {
        // Arrange
        var command = new CreateHubCommand(
            "My Bar",
            HubType.Venue,
            Guid.NewGuid(),
            new HubSettings());

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.SaveChanges(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallAddBeforeSaveChanges_WhenProcessing()
    {
        // Arrange
        var command = new CreateHubCommand(
            "My Bar",
            HubType.Venue,
            Guid.NewGuid(),
            new HubSettings());

        var callOrder = new List<string>();

        _hubRepositoryMock
            .Setup(r => r.Add(It.IsAny<HubEntity>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Add"))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().ContainInOrder("Add", "SaveChanges");
    }

    [Fact]
    public async Task Handle_ShouldCreatePortableHub_WhenTypeIsPortable()
    {
        // Arrange
        var command = new CreateHubCommand(
            "Road Trip",
            HubType.Portable,
            Guid.NewGuid(),
            new HubSettings());

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _hubRepositoryMock.Verify(
            r => r.Add(It.Is<HubEntity>(h => h.Type == HubType.Portable),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_WhenProvided()
    {
        // Arrange
        var command = new CreateHubCommand(
            "My Bar",
            HubType.Venue,
            Guid.NewGuid(),
            new HubSettings());

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, token);

        // Assert
        _hubRepositoryMock.Verify(
            r => r.Add(It.IsAny<HubEntity>(), token),
            Times.Once);
        _unitOfWorkMock.Verify(
            u => u.SaveChanges(token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldApplyCustomSettings_WhenProvided()
    {
        // Arrange
        var settings = new HubSettings
        {
            AllowProposals = false,
            AutoSkipThreshold = 0.50,
            VotingWindowSeconds = 30,
            MaxQueueSize = 100
        };

        var command = new CreateHubCommand(
            "My Bar",
            HubType.Venue,
            Guid.NewGuid(),
            settings);

        HubEntity? capturedHub = null;
        _hubRepositoryMock
            .Setup(r => r.Add(It.IsAny<HubEntity>(), It.IsAny<CancellationToken>()))
            .Callback<HubEntity, CancellationToken>((h, _) => capturedHub = h)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedHub.Should().NotBeNull();
        capturedHub!.Settings.AllowProposals.Should().BeFalse();
        capturedHub.Settings.AutoSkipThreshold.Should().Be(0.50);
        capturedHub.Settings.VotingWindowSeconds.Should().Be(30);
        capturedHub.Settings.MaxQueueSize.Should().Be(100);
    }
}
