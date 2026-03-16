using FluentAssertions;
using Musicratic.Auth.Domain.Entities;
using Musicratic.Auth.Domain.Events;

namespace Musicratic.Auth.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Create_ShouldReturnUser_WhenValidParameters()
    {
        // Arrange
        var sub = "auth|12345";
        var displayName = "TestUser";
        var email = "test@example.com";
        var avatar = "https://cdn.example.com/avatar.png";

        // Act
        var user = User.Create(sub, displayName, email, avatar);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.AuthentikSub.Should().Be(sub);
        user.DisplayName.Should().Be(displayName);
        user.Email.Should().Be(email);
        user.AvatarUrl.Should().Be(avatar);
        user.WalletBalance.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldSetAvatarToNull_WhenNotProvided()
    {
        // Act
        var user = User.Create("auth|12345", "TestUser", "test@example.com");

        // Assert
        user.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseUserCreatedEvent_WhenCreated()
    {
        // Act
        var user = User.Create("auth|12345", "TestUser", "test@example.com");

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedEvent>()
            .Which.Should().Match<UserCreatedEvent>(e =>
                e.UserId == user.Id && e.Email == "test@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenAuthentikSubIsInvalid(string? sub)
    {
        // Act
        var act = () => User.Create(sub!, "TestUser", "test@example.com");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenDisplayNameIsInvalid(string? displayName)
    {
        // Act
        var act = () => User.Create("auth|12345", displayName!, "test@example.com");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenEmailIsInvalid(string? email)
    {
        // Act
        var act = () => User.Create("auth|12345", "TestUser", email!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreditWallet_ShouldIncreaseBalance_WhenAmountIsPositive()
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");
        user.ClearDomainEvents();

        // Act
        user.CreditWallet(100);

        // Assert
        user.WalletBalance.Should().Be(100);
    }

    [Fact]
    public void CreditWallet_ShouldRaiseWalletCreditedEvent_WhenCredited()
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");
        user.ClearDomainEvents();

        // Act
        user.CreditWallet(50);

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<WalletCreditedEvent>()
            .Which.Should().Match<WalletCreditedEvent>(e =>
                e.UserId == user.Id && e.Amount == 50 && e.NewBalance == 50);
    }

    [Fact]
    public void CreditWallet_ShouldAccumulate_WhenCalledMultipleTimes()
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");

        // Act
        user.CreditWallet(100);
        user.CreditWallet(50);

        // Assert
        user.WalletBalance.Should().Be(150);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CreditWallet_ShouldThrow_WhenAmountIsNotPositive(int amount)
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");

        // Act
        var act = () => user.CreditWallet(amount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DebitWallet_ShouldDecreaseBalance_WhenSufficientFunds()
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");
        user.CreditWallet(100);
        user.ClearDomainEvents();

        // Act
        user.DebitWallet(30);

        // Assert
        user.WalletBalance.Should().Be(70);
    }

    [Fact]
    public void DebitWallet_ShouldRaiseWalletDebitedEvent_WhenDebited()
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");
        user.CreditWallet(100);
        user.ClearDomainEvents();

        // Act
        user.DebitWallet(30);

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<WalletDebitedEvent>()
            .Which.Should().Match<WalletDebitedEvent>(e =>
                e.UserId == user.Id && e.Amount == 30 && e.NewBalance == 70);
    }

    [Fact]
    public void DebitWallet_ShouldThrow_WhenInsufficientBalance()
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");
        user.CreditWallet(10);

        // Act
        var act = () => user.DebitWallet(50);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient wallet balance*");
    }

    [Fact]
    public void DebitWallet_ShouldThrow_WhenBalanceIsZero()
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");

        // Act
        var act = () => user.DebitWallet(1);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void DebitWallet_ShouldThrow_WhenAmountIsNotPositive(int amount)
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");
        user.CreditWallet(100);

        // Act
        var act = () => user.DebitWallet(amount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DebitWallet_ShouldAllowExactBalance_WhenAmountEqualsBalance()
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");
        user.CreditWallet(50);
        user.ClearDomainEvents();

        // Act
        user.DebitWallet(50);

        // Assert
        user.WalletBalance.Should().Be(0);
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateDisplayNameAndAvatar_WhenValidInput()
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");

        // Act
        user.UpdateProfile("NewName", "https://cdn.example.com/new-avatar.png");

        // Assert
        user.DisplayName.Should().Be("NewName");
        user.AvatarUrl.Should().Be("https://cdn.example.com/new-avatar.png");
    }

    [Fact]
    public void UpdateProfile_ShouldClearAvatar_WhenAvatarIsNull()
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com", "https://cdn.example.com/avatar.png");

        // Act
        user.UpdateProfile("NewName", null);

        // Assert
        user.DisplayName.Should().Be("NewName");
        user.AvatarUrl.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_ShouldThrow_WhenDisplayNameIsInvalid(string? displayName)
    {
        // Arrange
        var user = User.Create("auth|12345", "TestUser", "test@example.com");

        // Act
        var act = () => user.UpdateProfile(displayName!, null);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
