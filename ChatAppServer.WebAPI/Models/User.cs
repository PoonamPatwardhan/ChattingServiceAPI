using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAppServer.WebAPI.Models;

public class User
{
    public User()
    {
        
    }
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
