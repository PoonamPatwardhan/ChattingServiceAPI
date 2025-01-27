using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using GenericFileService.Files;
using MongoDB.Driver;

namespace ChatAppServer.WebAPI.Services;

public interface IUsersService
{
    Task<List<User>> GetUsersAsync(CancellationToken cancellationToken);
    Task<User> CreateUserAsync(RegisterDto request, CancellationToken cancellationToken);
    User? GetUserByName(string name, CancellationToken cancellationToken);
    User? GetUserById(string userId);
    Task UpdateUserStatus(string userId, string newStatus);

}

public class UsersService :IUsersService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoDatabase _mongoDatabase;

    public UsersService(DatabaseProviderService databaseProvider)
    {
        _mongoDatabase = databaseProvider.GetAccess();
        _users = _mongoDatabase.GetCollection<User>("Users");
    }

    public async Task<User> CreateUserAsync(RegisterDto request, CancellationToken cancellationToken)
    {
        var userExisting = _users.Find(x => x.Name.Equals(request.Name)).FirstOrDefault(cancellationToken);

        if (userExisting != null)
        {
            throw new InvalidOperationException("Name already exists, choose another.");
        }

        string avatar = FileService.FileSaveToServer(request.File, "wwwroot/avatar/");

        User user = new()
        {
            Name = request.Name,
            Avatar = avatar
        };
        await _users.InsertOneAsync(user);

        return user;
    }


    public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken)
    {
        return await _users.Find(_ => true).ToListAsync(cancellationToken);
    }

    public User? GetUserByName(string username, CancellationToken cancellationToken)
    {
        return _users.Find(x => x.Name.Equals(username)).FirstOrDefault(cancellationToken);
    }

    public User? GetUserById(string userId)
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
