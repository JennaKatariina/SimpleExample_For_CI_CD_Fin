using FluentAssertions;
using Moq;
using SimpleExample.Application.DTOs;
using SimpleExample.Application.Interfaces;
using SimpleExample.Application.Services;
using SimpleExample.Domain.Entities;
using Xunit;

namespace SimpleExample.Tests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _service = new UserService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        CreateUserDto dto = new CreateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        // Mock: Email ei ole käytössä
        _mockRepository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        UserDto result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Matti");
        result.LastName.Should().Be("Meikäläinen");
        result.Email.Should().Be("matti@example.com");

        // Varmista että AddAsync kutsuttiin kerran
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        CreateUserDto dto = new CreateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "existing@example.com"
        };

        User existingUser = new User("Maija", "Virtanen", "existing@example.com");

        // Mock: Email on jo käytössä!
        _mockRepository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*jo olemassa*");

        // Varmista että AddAsync EI kutsuttu
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        User user = new User("Matti", "Meikäläinen", "matti@example.com")
        {
            Id = id
        };

        // Mockataan repository palauttamaan käyttäjä, kun haetaan ID:llä
        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(user);

        // Act
        UserDto? result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.FirstName.Should().Be("Matti");
        result.LastName.Should().Be("Meikäläinen");
        result.Email.Should().Be("matti@example.com");

        _mockRepository.Verify(x => x.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        Guid id = Guid.NewGuid();

        // Mockataan repository palauttamaan null, kun haetaan ID:llä, jota ei ole
        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((User?)null);

        UserDto? result = await _service.GetByIdAsync(id);

        result.Should().BeNull();
        _mockRepository.Verify(x => x.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User("Matti", "Meikäläinen", "matti@example.com") { Id = Guid.NewGuid() },
            new User("Maija", "Virtanen", "maija@example.com") { Id = Guid.NewGuid() }
        };

        // Mockataan repository palauttamaan lista käyttäjistä
        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        IEnumerable<UserDto> result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(x => x.Email).Should().BeEquivalentTo(new[] { "matti@example.com", "maija@example.com" });

        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_ShouldUpdateAndReturnUser()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        User existing = new User("Matti", "Meikäläinen", "matti@example.com")
        {
            Id = id
        };

        UpdateUserDto dto = new UpdateUserDto
        {
            FirstName = "Matti2",
            LastName = "Meikäläinen2",
            Email = "matti2@example.com"
        };

        // Mockataan repository palauttamaan olemassa oleva käyttäjä ID:llä
        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(existing);

        // Mockataan repository palauttamaan null, kun haetaan uudella email:lla (email on vapaa)
        _mockRepository
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        // Mockataan repository päivittämään käyttäjä ja palauttamaan päivitetty käyttäjä
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        UserDto? result = await _service.UpdateAsync(id, dto);

        // Assert
        result.Id.Should().Be(id);
        result.FirstName.Should().Be("Matti2");
        result.LastName.Should().Be("Meikäläinen2");
        result.Email.Should().Be("matti2@example.com");

        _mockRepository.Verify(x => x.GetByIdAsync(id), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        Guid id = Guid.NewGuid();
        UpdateUserDto dto = new UpdateUserDto
        {
            FirstName = "Uusi",
            LastName = "Nimi",
            Email = "uusi@example.com"
        };

        // Mockataan repository palauttamaan null, kun haetaan ID:llä, jota ei ole
        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((User?)null);

        UserDto? result = await _service.UpdateAsync(id, dto);

        result.Should().BeNull();
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Mockataan repository palauttamaan true, kun tarkistetaan onko käyttäjä olemassa
        _mockRepository
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(true);

        // Mockataan repository poistamaan käyttäjä
        _mockRepository
            .Setup(x => x.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _service.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        Guid id = Guid.NewGuid();

        // Mockataan repository palauttamaan null, kun haetaan ID:llä, jota ei ole
        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((User?)null);

        bool result = await _service.DeleteAsync(id);

        result.Should().BeFalse();
        _mockRepository.Verify(x => x.DeleteAsync(id), Times.Never);
    }
}