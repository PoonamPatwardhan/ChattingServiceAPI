using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using GenericFileService.Files;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ChatAppServer.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public sealed class AuthController : ControllerBase
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoDatabase _mongoDatabase;

    public AuthController(DatabaseProviderService databaseProvider)
    {
        _mongoDatabase = databaseProvider.GetAccess();
        _users = _mongoDatabase.GetCollection<User>("Users");
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterDto request, CancellationToken cancellationToken)
    {
        var userExisting = _users.Find(x => x.Name.Equals(request.Name)).FirstOrDefault(cancellationToken);

        if (userExisting != null)
        {
            return BadRequest(new { Message = "Name already exists, choose another" });
        }

        string avatar = FileService.FileSaveToServer(request.File, "wwwroot/avatar/");

        User user = new()
        {
            Name = request.Name,
            Avatar = avatar
        };
        await _users.InsertOneAsync(user);
        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> Login(string name, CancellationToken cancellationToken)
    {
        User? user = _users.Find(x => x.Name.Equals(name)).FirstOrDefault();
        user.Status = "online";

        return Ok(user);
    }
}

