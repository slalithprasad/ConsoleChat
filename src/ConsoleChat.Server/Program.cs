using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ConsoleChat.Server.Models;

Dictionary<string, Room> Rooms = new();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression();

WebApplication app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseResponseCompression();

app.UseWebSockets();

app.MapHealthChecks("/health");

app.MapGet("/", () => "Console Chat Server is Operational.");

app.MapPost("/room", async (HttpContext context) =>
{
    var roomId = Guid.NewGuid().ToString();

    Rooms[roomId] = new Room { RoomId = roomId };

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new { RoomId = roomId });
});

app.Map("/chat", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var roomId = context.Request.Query["roomId"].ToString();

        if (string.IsNullOrWhiteSpace(roomId))
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Room ID required", CancellationToken.None);
            return;
        }

        if (!Rooms.ContainsKey(roomId))
        {
            Rooms[roomId] = new Room { RoomId = roomId };
        }
        Rooms[roomId].Clients.Add(webSocket);

        var message = new Message { RoomId = roomId };
        await HandleWebSocketConnection(webSocket, message);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.Run();

#region Handle Websocket Connection
async Task HandleWebSocketConnection(WebSocket webSocket, Message message)
{
    var buffer = new byte[1024 * 4];

    while (webSocket.State == WebSocketState.Open)
    {
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            Rooms[message.RoomId!].Clients.Remove(webSocket);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            break;
        }
        else if (result.MessageType == WebSocketMessageType.Text)
        {
            var receivedMessageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var receivedMessage = JsonSerializer.Deserialize<Message>(receivedMessageJson);

            if (receivedMessage is not null)
            {
                Console.WriteLine($"[{message.RoomId}] {receivedMessage.UserName}: {receivedMessage.Text}");

                receivedMessage.RoomId = message.RoomId;

                await BroadcastMessageToRoom(receivedMessage);
            }
        }
    }
}

async Task BroadcastMessageToRoom(Message message)
{
    if (message.RoomId != null && Rooms.TryGetValue(message.RoomId, out var room))
    {
        var messageJson = JsonSerializer.Serialize(message);
        var messageBuffer = Encoding.UTF8.GetBytes(messageJson);

        foreach (var client in room.Clients)
        {
            if (client.State == WebSocketState.Open)
            {
                await client.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
#endregion