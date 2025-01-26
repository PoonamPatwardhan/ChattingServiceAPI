using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAppServer.WebAPI.Models;
// Reference - https://www.mongodb.com/community/forums/t/scalable-group-chat-modelling-and-performance/124623/2
public class Message
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } //check if Guid is needed

    [BsonElement("senderId")]
    //[BsonGuidRepresentation(GuidRepresentation.Standard)]
    public string SenderId { get; set; } //TODO change to Guid after integrating with main app

    [BsonElement("senderUsername")]
    public string SenderUsername { get; set; }

    [BsonElement("receiverId")]
    public string ReceiverId { get; set; } //TODO change to Guid after integrating with main app

    [BsonElement("receiverUsername")]
    public string ReceiverUsername { get; set; }

    [BsonElement("content")]
    public required string Content { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("isDelivered")]
    public bool IsDelivered { get; set; } = false;

    [BsonElement("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    [BsonElement("readAt")]
    public DateTime? ReadAt { get; set; }

    // for future feature if needed
    /*public bool SenderDeleted { get; set; }
    public bool RecipientDeleted { get; set; }*/
}