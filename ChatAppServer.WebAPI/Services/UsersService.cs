using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using GenericFileService.Files;
using MongoDB.Driver;

namespace ChatAppServer.WebAPI.Services;

public class UsersService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoDatabase _mongoDatabase;

    public UsersService(DatabaseProviderService databaseProvider)
    {
        _mongoDatabase = databaseProvider.GetAccess();
        _users = _mongoDatabase.GetCollection<User>("Users");
    }

    public async Task<User> CreateUser(RegisterDto request, CancellationToken cancellationToken)
    {
        string avatar = FileService.FileSaveToServer(request.File, "wwwroot/avatar/");

        User newUser = new()
        {
            Name = request.Name,
            Avatar = avatar
        };
        await _users.InsertOneAsync(newUser, cancellationToken: cancellationToken);

        return newUser;
    }

    public User GetUserByName(string username, CancellationToken cancellationToken)
    {
        return _users.Find(x => x.Name.Equals(username)).FirstOrDefault(cancellationToken);
    }

    public User GetUserById(string userId)
    {
        return _users.Find(x => x.Id != null && x.Id.Equals(userId)).FirstOrDefault();
    }

    public async Task UpdateUserStatus(string userId, string newStatus)
    {
        var userWithGivenId = Builders<User>.Filter.Eq(user => user.Id, userId);
        var updateStatusToOnline = Builders<User>.Update.Set(user => user.Status, newStatus);
        await _users.UpdateOneAsync(userWithGivenId, updateStatusToOnline);
    }
}
