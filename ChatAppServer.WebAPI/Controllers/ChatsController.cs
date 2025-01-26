using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using ChatAppServer.WebAPI.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace ChatAppServer.WebAPI.Controllers;
[Route("api/[controller]/[action]")]
[ApiController]
public sealed class ChatsController : ControllerBase
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IMongoCollection<Message> _messages;
    private readonly IMongoCollection<User> _users;

    private readonly IHubContext<ChatHub> _hubContext;

    public ChatsController(IHubContext<ChatHub> hubContext, DatabaseProviderService databaseProvider)
    {
        _mongoDatabase = databaseProvider.GetAccess();
        _messages = _mongoDatabase.GetCollection<Message>("Chats");
        _users = _mongoDatabase.GetCollection<User>("Users");
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        List<User> users = await _users.Find(_ => true).ToListAsync(); 
        return Ok(users);
    }

    //TODO use PagedList<MessageDto> later
    [HttpGet]
    public async Task<ActionResult<MessageDto>> GetChats(string senderName, string receiverName, CancellationToken cancellationToken)
    {
        var chats = await _messages.Find(x => x.SenderUsername == senderName && x.ReceiverUsername == receiverName ||
                                           x.ReceiverUsername == senderName && x.SenderUsername == receiverName).ToListAsync(cancellationToken);

        var chatsDto = chats.OrderBy(x => x.CreatedAt).Select(message => 
            new MessageDto()
            {
                SenderUsername = message.SenderId,
                ReceiverUsername = message.ReceiverId,
                CreatedAt = message.CreatedAt,
                Content = message.Content
            }).ToList();

        return Ok(chatsDto);
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage(SendMessageDto request, CancellationToken cancellationToken)
    {
        var sender =  _users.Find(x => x.Id != null && x.Id.Equals(request.SenderId)).FirstOrDefault();
        var recipient =  _users.Find(x => x.Id != null && x.Id.Equals(request.ReceiverId)).FirstOrDefault();

        if (recipient == null)
        {
            return NotFound("No user found to send message to");
        }

        var message = new Message
        {
            SenderId = sender.Id,
            SenderUsername = sender.Name,
            ReceiverId = recipient.Id,
            ReceiverUsername = recipient.Name,
            Content = request.Message,
            CreatedAt = DateTime.Now
        };

        await _messages.InsertOneAsync(message, cancellationToken: cancellationToken);
        var messageDto = new MessageDto()
        {
            SenderUsername = message.SenderId,
            ReceiverUsername = message.ReceiverId,
            CreatedAt = message.CreatedAt,
            Content = message.Content
        };
        var connections = await ConnectionTracker.GetConnectionsForUser(request.ReceiverId);
        if (connections != null && connections.Any())
        {
            await _hubContext.Clients.Clients(connections).SendAsync("Messages", messageDto, cancellationToken: cancellationToken);
        }
        
        return Ok(messageDto);
    }
}
