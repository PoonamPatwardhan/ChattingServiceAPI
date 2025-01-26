using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ChatAppServer.WebAPI;

public class DatabaseProviderService
{
    private readonly IMongoDatabase _mongoDatabase;

    public DatabaseProviderService(IOptions<DatabaseSettings> messengerDatabaseSettings)
    {
        var mongoClient = new MongoClient(messengerDatabaseSettings.Value.ConnectionString);
        _mongoDatabase = mongoClient.GetDatabase(messengerDatabaseSettings.Value.DatabaseName);
    }

    public IMongoDatabase GetAccess() => _mongoDatabase;
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}
