using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using ChatAppServer.WebAPI.Services;
using GenericFileService.Files;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ChatAppServer.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUsersService _usersService;

    public AuthController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterDto request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _usersService.CreateUserAsync(request, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Login(string name, CancellationToken cancellationToken)
    {
        var user = _usersService.GetUserByName(name, cancellationToken);

        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        await _usersService.UpdateUserStatus(user.Id, "online");
        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _usersService.GetUsersAsync(cancellationToken);
        return Ok(users);
    }
}


