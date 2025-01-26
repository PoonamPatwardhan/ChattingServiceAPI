namespace ChatAppServer.WebAPI.Dtos;


public sealed record SendMessageDto(
    string SenderId,
    string ReceiverId,
    string Message);
