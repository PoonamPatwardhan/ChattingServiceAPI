namespace ChatAppServer.WebAPI.Dtos;

public class MessageDto
{
    public string SenderUsername { get; set; }  //string SenderId ??
    public string ReceiverUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Content { get; set; }
}
