using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using ChatAppServer.WebAPI.Services;
using ChatAppServer.WebAPI.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace ChatAppServer.WebAPI.Controllers;
[Route("api/[controller]/[action]")]
[ApiController]
public sealed class ChatsController : ControllerBase
{
    private readonly IMessagesService _messagesService;

    public ChatsController(IMessagesService messagesService)
    {
        _messagesService = messagesService;
    }

    //TODO use PagedList<MessageDto> later
    [HttpGet]
    public async Task<IActionResult> GetChats(string senderName, string receiverName, CancellationToken cancellationToken)
    {
        var chats = await _messagesService.GetMessagesAsync(senderName, receiverName, cancellationToken);
        return Ok(chats);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request, CancellationToken cancellationToken)
    {
        try
        {
            var message = await _messagesService.SendMessageAsync(request, cancellationToken);
            return Ok(message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
