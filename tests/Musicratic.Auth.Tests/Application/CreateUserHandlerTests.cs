using FluentAssertions;
using Moq;
using Musicratic.Auth.Application;
using Musicratic.Auth.Application.Commands.CreateUser;
using Musicratic.Auth.Domain.Entities;
using Musicratic.Auth.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Tests.Application;

public class CreateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthUnitOfWork> _unitOfWorkMock;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IAuthUnitOfWork>();
        _handler = new CreateUserHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserId_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateUserCommand(
            "auth|12345",
            "TestUser",
            "test@example.com",
            "https://cdn.example.com/avatar.png");

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
        var command = new CreateUserCommand(
            "auth|12345",
            "TestUser",
            "test@example.com",
            null);

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.Add(It.Is<User>(u =>
                u.AuthentikSub == "auth|12345" &&
                u.DisplayName == "TestUser" &&
                u.Email == "test@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenUserIsAdded()
    {
        // Arrange
        var command = new CreateUserCommand(
            "auth|12345",
            "TestUser",
            "test@example.com",
            null);

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
        var command = new CreateUserCommand(
            "auth|12345",
            "TestUser",
            "test@example.com",
            null);

        var callOrder = new List<string>();

        _userRepositoryMock
            .Setup(r => r.Add(It.IsAny<User>(), It.IsAny<CancellationToken>()))
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
    public async Task Handle_ShouldPassAvatarUrl_WhenProvided()
    {
        // Arrange
        var avatarUrl = "https://cdn.example.com/avatar.png";
        var command = new CreateUserCommand(
            "auth|12345",
            "TestUser",
            "test@example.com",
            avatarUrl);

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.Add(It.Is<User>(u => u.AvatarUrl == avatarUrl),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_WhenProvided()
    {
        // Arrange
        var command = new CreateUserCommand(
            "auth|12345",
            "TestUser",
            "test@example.com",
            null);

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _unitOfWorkMock
            .Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, token);

        // Assert
        _userRepositoryMock.Verify(
            r => r.Add(It.IsAny<User>(), token),
            Times.Once);
        _unitOfWorkMock.Verify(
            u => u.SaveChanges(token),
            Times.Once);
    }
}
