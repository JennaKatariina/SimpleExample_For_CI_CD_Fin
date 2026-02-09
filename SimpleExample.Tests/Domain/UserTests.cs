using FluentAssertions;
using SimpleExample.Domain.Entities;
using Xunit;

namespace SimpleExample.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        // Act
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        // Assert
        user.Should().NotBeNull();
        user.FirstName.Should().Be("Matti");
        user.LastName.Should().Be("Meikäläinen");
        user.Email.Should().Be("matti@example.com");
    }

    [Fact]
    public void Constructor_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => new User("", "Meikäläinen", "test@test.com");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Etunimi ei voi olla tyhjä*");
    }

    [Fact]
    public void Constructor_WithTooShortFirstName_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => new User("AB", "Meikäläinen", "test@test.com");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Etunimen tulee olla vähintään 3 merkkiä pitkä*");
    }

    [Fact]
    public void Constructor_WithEmptyLastName_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => new User("Matti", "", "test@test.com");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Sukunimi ei voi olla tyhjä*");
    }

    [Fact]
    public void Constructor_WithTooShortLastName_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => new User("Matti", "XY", "test@test.com");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Sukunimen tulee olla vähintään 3 merkkiä pitkä*");
    }

    [Fact]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => new User("Matti", "Meikäläinen", "invalid-email");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Sähköpostin tulee olla kelvollinen*");
    }

    [Fact]
    public void Constructor_WithNullFirstName_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new User(null!, "Meikäläinen", "test@test.com");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("Mat")]  // Minimiraja 3 merkkiä
    [InlineData("Matti")]
    [InlineData("MattiJohannes")]
    public void Constructor_WithValidFirstNameLengths_ShouldSucceed(string firstName)
    {
        // Act
        User user = new User(firstName, "Meikäläinen", "test@test.com");

        // Assert
        user.FirstName.Should().Be(firstName);
    }

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        // Act
        user.UpdateBasicInfo("Maija", "Virtanen");

        // Assert
        user.FirstName.Should().Be("Maija");
        user.LastName.Should().Be("Virtanen");
    }

    [Fact]
    public void UpdateBasicInfo_WithTooShortFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        // Act
        Action act = () => user.UpdateBasicInfo("AB", "Virtanen");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Etunimen tulee olla vähintään 3 merkkiä pitkä*");
    }

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldUpdateEmail()
    {
        // Arrange
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        // Act
        user.UpdateEmail("uusi@example.com");

        // Assert
        user.Email.Should().Be("uusi@example.com");
    }

    [Fact]
    public void UpdateEmail_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Arrange
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        // Act
        Action act = () => user.UpdateEmail("invalid-email");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Sähköpostin tulee olla kelvollinen*");
    }
}