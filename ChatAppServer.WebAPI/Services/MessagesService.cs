using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using ChatAppServer.WebAPI.SignalR;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace ChatAppServer.WebAPI.Services;

public interface IMessagesService
{
    Task<List<MessageDto>> GetMessagesAsync(string senderName, string receiverName, CancellationToken cancellationToken);
    Task<MessageDto> SendMessageAsync(SendMessageDto request, CancellationToken cancellationToken);
}

public class MessagesService : IMessagesService
{
    private readonly IMongoCollection<Message> _messages;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IUsersService _usersService;

    public MessagesService(DatabaseProviderService databaseProvider, IHubContext<ChatHub> hubContext, IUsersService usersService)
    {
        var database = databaseProvider.GetAccess();
        _messages = database.GetCollection<Message>("Chats");
        _hubContext = hubContext;
        _usersService = usersService;
    }

    public async Task<List<MessageDto>> GetMessagesAsync(string senderName, string receiverName, CancellationToken cancellationToken)
    {
        var chats = await _messages
            .Find(x => (x.SenderUsername == senderName && x.ReceiverUsername == receiverName) ||
                       (x.ReceiverUsername == senderName && x.SenderUsername == receiverName))
            .ToListAsync(cancellationToken);

        return chats
            .OrderBy(x => x.CreatedAt)
            .Select(message => new MessageDto
            {
                SenderUsername = message.SenderId,
                ReceiverUsername = message.ReceiverId,
                CreatedAt = message.CreatedAt,
                Content = message.Content
            })
            .ToList();
    }

    public async Task<MessageDto> SendMessageAsync(SendMessageDto request, CancellationToken cancellationToken)
    {
        var sender = _usersService.GetUserById(request.SenderId);
        if (sender == null)
        {
            throw new KeyNotFoundException("Sender not found.");
        }

        var recipient = _usersService.GetUserById(request.ReceiverId);
        if (recipient == null)
        {
            throw new KeyNotFoundException("Recipient not found.");
        }

        var message = new Message
        {
            SenderId = sender.Id,
            SenderUsername = sender.Name,
            ReceiverId = recipient.Id,
            ReceiverUsername = recipient.Name,
            Content = request.Message,
            CreatedAt = DateTime.UtcNow
        };

        await _messages.InsertOneAsync(message);

        var messageDto = new MessageDto
        {
            SenderUsername = message.SenderId,
            ReceiverUsername = message.ReceiverId,
            CreatedAt = message.CreatedAt,
            Content = message.Content
        };

        var connections = await ConnectionTracker.GetConnectionsForUser(request.ReceiverId);
        if (connections != null && connections.Any())
        {
            await _hubContext.Clients.Clients(connections)
                .SendAsync("Messages", messageDto, cancellationToken);
        }

        return messageDto;
    }
}
