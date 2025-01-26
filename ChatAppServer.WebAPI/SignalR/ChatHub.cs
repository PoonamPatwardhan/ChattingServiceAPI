using ChatAppServer.WebAPI.Models;
using ChatAppServer.WebAPI.Services;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace ChatAppServer.WebAPI.SignalR;

public sealed class ChatHub : Hub
{
    private readonly UsersService _usersService;
    private readonly ConnectionTracker _connectionTracker;

    public ChatHub(UsersService usersService, ConnectionTracker connectionTracker)
    {
        _usersService = usersService;
        _connectionTracker = connectionTracker;
    }
    
    public async Task Connect(string userId)
    {
        await _connectionTracker.UserConnected(userId, Context.ConnectionId);

        var user = _usersService.GetUserById(userId);
        if (user is not null)
        {
            await _usersService.UpdateUserStatus(user.Id, "online");
            await Clients.All.SendAsync("Users", user);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = await _connectionTracker.GetOnlineUserForConnection(Context.ConnectionId);
        
        var user = _usersService.GetUserById(userId);
        if (user is not null)
        {
            await _connectionTracker.UserDisconnected(userId, Context.ConnectionId);
            await _usersService.UpdateUserStatus(user.Id, "offline");
            await Clients.Others.SendAsync("Users", user);
        }
    }
}
/*public sealed class ChatHub : Hub
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ConnectionTracker _tracker;

    public ChatHub(DatabaseProviderService databaseProvider, ConnectionTracker tracker)
    {
        _mongoDatabase = databaseProvider.GetAccess();
        _users = _mongoDatabase.GetCollection<User>("Users");
        _tracker = tracker;
    }

    public async Task Connect(string userId)
    {
        await _tracker.UserConnected(userId, Context.ConnectionId);

        var currentUsers = await _tracker.GetOnlineUsers();

        var user = _users.Find(x => x.Id != null && x.Id.Equals(userId)).FirstOrDefault();
        if (user is not null)
        {
            user.Status = "online";

            await Clients.All.SendAsync("Users", user);
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = await _tracker.GetOnlineUserForConnection(Context.ConnectionId);

        var user = _users.Find(x => x.Id != null && x.Id.Equals(userId)).FirstOrDefault();
        if (user is not null)
        {
            user.Status = "offline";

            await Clients.Others.SendAsync("Users", user);
        }

        //await base.OnDisconnectedAsync(exception);
    }
}*/
