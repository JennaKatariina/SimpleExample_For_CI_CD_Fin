using Microsoft.AspNetCore.Mvc;
using SimpleExample.Application.DTOs;
using SimpleExample.Application.Interfaces;

namespace SimpleExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get all users - Updated via GitHub Actions!
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        IEnumerable<UserDto> users = await _userService.GetAllAsync();
        return Ok(users);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        UserDto? user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = $"User with ID {id} not found" });
        }
        return Ok(user);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        UserDto user = await _userService.CreateAsync(createUserDto);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        UserDto? user = await _userService.UpdateAsync(id, updateUserDto);
        if (user == null)
        {
            return NotFound(new { message = $"User with ID {id} not found" });
        }
        return Ok(user);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        bool deleted = await _userService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"User with ID {id} not found" });
        }
        return NoContent();
    }
}
