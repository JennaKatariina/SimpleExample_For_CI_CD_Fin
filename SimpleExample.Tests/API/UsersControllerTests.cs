using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleExample.API.Controllers;
using SimpleExample.Application.DTOs;
using SimpleExample.Application.Interfaces;
using Xunit;

namespace SimpleExample.Tests.API;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _controller = new UsersController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithUsers()
    {
        // Arrange
        List<UserDto> users = new List<UserDto>
        {
            new UserDto { Id = Guid.NewGuid(), FirstName = "Matti", LastName = "M", Email = "m@m.com" },
            new UserDto { Id = Guid.NewGuid(), FirstName = "Maija", LastName = "V", Email = "m@v.com" }
        };

        _mockService
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        ActionResult<IEnumerable<UserDto>> result = await _controller.GetAll();

        // Assert
        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        IEnumerable<UserDto> returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        returnedUsers.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WhenUserExists_ShouldReturnOk()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserDto user = new UserDto { Id = userId, FirstName = "Matti", LastName = "M", Email = "m@m.com" };

        _mockService
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        ActionResult<UserDto> result = await _controller.GetById(userId);

        // Assert
        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        UserDto returnedUser = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returnedUser.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetById_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        _mockService
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((UserDto?)null);

        // Act
        ActionResult<UserDto> result = await _controller.GetById(userId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        CreateUserDto createDto = new CreateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        UserDto createdUser = new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email
        };

        _mockService
            .Setup(x => x.CreateAsync(createDto))
            .ReturnsAsync(createdUser);

        // Act
        ActionResult<UserDto> result = await _controller.Create(createDto);

        // Assert
        CreatedAtActionResult createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        UserDto returnedUser = createdResult.Value.Should().BeOfType<UserDto>().Subject;
        returnedUser.FirstName.Should().Be("Matti");
    }

    [Fact]
    public async Task Create_WhenDuplicate_ShouldReturnConflict()
    {
        // Arrange
        CreateUserDto createDto = new CreateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "existing@example.com"
        };

        // Mockataan, että palvelu heittää InvalidOperationExceptionin, joka tarkoittaa, että käyttäjä on jo olemassa
        _mockService
            .Setup(x => x.CreateAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("User already exists"));

        // Act
        ActionResult<UserDto> result = await _controller.Create(createDto);

        // Assert
        ConflictObjectResult conflict = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflict.Value.Should().NotBeNull(); // { message = ... }
        _mockService.Verify(x => x.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Create_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        CreateUserDto createDto = new CreateUserDto
        {
            FirstName = "",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        // Mockataan, että palvelu heittää ArgumentExceptionin, joka tarkoittaa, että syötetty data on virheellistä
        _mockService
            .Setup(x => x.CreateAsync(createDto))
            .ThrowsAsync(new ArgumentException("Etunimi ei voi olla tyhjä."));

        // Act
        ActionResult<UserDto> result = await _controller.Create(createDto);

        // Assert
        BadRequestObjectResult badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().NotBeNull(); // { message = ... }
        _mockService.Verify(x => x.CreateAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task Update_WhenSuccess_ShouldReturnOk()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        UpdateUserDto updateDto = new UpdateUserDto
        {
            FirstName = "Matti",
            LastName = "Updated",
            Email = "matti.updated@example.com"
        };

        UserDto updated = new UserDto
        {
            Id = id,
            FirstName = updateDto.FirstName,
            LastName = updateDto.LastName,
            Email = updateDto.Email
        };

        // Mockataan, että palvelu palauttaa päivitetyn käyttäjätiedon
        _mockService
            .Setup(x => x.UpdateAsync(id, updateDto))
            .ReturnsAsync(updated);

        // Act
        ActionResult<UserDto> result = await _controller.Update(id, updateDto);

        // Assert
        OkObjectResult ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        UserDto returned = ok.Value.Should().BeOfType<UserDto>().Subject;
        returned.Id.Should().Be(id);
        returned.Email.Should().Be("matti.updated@example.com");
        _mockService.Verify(x => x.UpdateAsync(id, updateDto), Times.Once);
    }

    [Fact]
    public async Task Update_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        UpdateUserDto updateDto = new UpdateUserDto
        {
            FirstName = "Matti",
            LastName = "Updated",
            Email = "matti.updated@example.com"
        };

        // Mockataan, palvelu palauttamaab null, päivitettävää käyttäjää ei löytynyt
        _mockService
            .Setup(x => x.UpdateAsync(id, updateDto))
            .ReturnsAsync((UserDto?)null);

        // Act
        ActionResult<UserDto> result = await _controller.Update(id, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(x => x.UpdateAsync(id, updateDto), Times.Once);
    }

    [Fact]
    public async Task Update_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        UpdateUserDto updateDto = new UpdateUserDto
        {
            FirstName = "Ma", // invalid
            LastName = "Updated",
            Email = "matti.updated@example.com"
        };

        // Mockataan, että palvelu heittää ArgumentExceptionin, syötetty data on virheellistä
        _mockService
            .Setup(x => x.UpdateAsync(id, updateDto))
            .ThrowsAsync(new ArgumentException("Etunimen tulee olla vähintään 3 merkkiä pitkä."));

        // Act
        ActionResult<UserDto> result = await _controller.Update(id, updateDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _mockService.Verify(x => x.UpdateAsync(id, updateDto), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenDeleted_ShouldReturnNoContent()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Mockataan palvelu palauttamaan true, käyttäjä onnistuneesti poistettu
        _mockService
            .Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(true);

        // Act
        ActionResult result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockService.Verify(x => x.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Mockataan palvelu palauttamaan false, käyttäjää ei löytynyt eikä siten poistettu
        _mockService
            .Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(false);

        // Act
        ActionResult result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(x => x.DeleteAsync(id), Times.Once);
    }
}