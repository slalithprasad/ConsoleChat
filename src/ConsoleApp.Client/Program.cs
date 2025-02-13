using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ConsoleApp.Client.Models;

string baseUrl = Environment.GetEnvironmentVariable("CONSOLECHAT_SERVERURI")!;

List<Message> messages = new List<Message>();

bool isHealthy = await CheckServerHealthAsync().ConfigureAwait(false);

if (!isHealthy)
{
    Console.WriteLine("Unable to connect to the server. Please try again later.");
}

Console.Write("Enter the username: ");
string? userName = Console.ReadLine();

while (string.IsNullOrEmpty(userName))
{
    Console.Write("Invalid username, please enter again: ");
    userName = Console.ReadLine();
}

Console.Write("Enter an option ('1' for Room Creation or '2' for Joining the Room): ");

string? roomOption = Console.ReadLine();

while (string.IsNullOrEmpty(roomOption) || !(roomOption != "1" || roomOption != "2"))
{
    Console.Write("Invalid option, enter 1 for Creating a Room and 2 for Joining the room: ");
    roomOption = Console.ReadLine();
}

string? roomId = null;

switch (roomOption)
{
    case "1":
        roomId = await CreateRoomAsync().ConfigureAwait(false);
        break;

    case "2":
        Console.Write("Enter the room id: ");
        roomId = Console.ReadLine();

        while (string.IsNullOrEmpty(roomId))
        {
            Console.Write("Invalid room id, please enter again: ");
            roomId = Console.ReadLine();
        }
        break;
}

Console.WriteLine($"\n====================================");
Console.WriteLine($"Connecting to Room: {roomId}");

var uri = new Uri(baseUrl);
var host = uri.Host;

using ClientWebSocket webSocket = new ClientWebSocket();
await webSocket.ConnectAsync(new Uri($"wss://{host}/chat?roomId={roomId}"), CancellationToken.None);

Console.WriteLine("Connected! Start chatting...");
Console.WriteLine($"====================================\n");



_ = Task.Run(() => ReceiveMessagesAsync(webSocket));

while (webSocket.State == WebSocketState.Open)
{
    string? messageText = Console.ReadLine();

    if (string.IsNullOrEmpty(messageText))
        continue;

    var message = new Message
    {
        RoomId = roomId,
        UserName = userName,
        Text = messageText
    };

    string jsonMessage = JsonSerializer.Serialize(message);
    byte[] messageBuffer = Encoding.UTF8.GetBytes(jsonMessage);

    if (webSocket.State == WebSocketState.Open)
    {
        await webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
    }
    else
    {
        Console.WriteLine("Connection closed. Unable to send the message.");
        break;
    }
}

Console.WriteLine("Disconnected.");

#region Print Messages
void PrintMessages(List<Message> messages)
{
    Console.Clear();

    int windowWidth = Console.WindowWidth;
    int windowHeight = Console.WindowHeight;

    Console.SetCursorPosition(0, 0);
    for (int i = 0; i < windowHeight; i++)
    {
        Console.WriteLine(new string(' ', windowWidth));
    }
    Console.SetCursorPosition(0, 0);

    int startLine = Math.Max(0, messages.Count - windowHeight + 1);
    for (int i = startLine; i < messages.Count; i++)
    {
        var message = messages[i];
        string textToDisplay = message.UserName == userName
            ? $"[You]: {message.Text}"
            : $"[{message.UserName}]: {message.Text}";

        if (message.UserName == userName)
            Console.ForegroundColor = ConsoleColor.Green;
        else
            Console.ResetColor();

        Console.WriteLine(textToDisplay);
    }

    Console.ResetColor();
    Console.SetCursorPosition(0, windowHeight - 1);
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("You: ");
    Console.ResetColor();
}
#endregion


#region API calls
async Task<bool> CheckServerHealthAsync()
{
    try
    {

        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        using HttpResponseMessage responseMessage = await client.GetAsync($"{baseUrl}health");

        responseMessage.EnsureSuccessStatusCode();

        string response = await responseMessage.Content.ReadAsStringAsync();

        return response.Equals("healthy", StringComparison.OrdinalIgnoreCase);
    }
    catch
    {
        return false;
    }
}

async Task<string?> CreateRoomAsync()
{
    try
    {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        using HttpResponseMessage response = await client.PostAsync($"{baseUrl}room", null);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var roomResponse = JsonSerializer.Deserialize<RoomResponse>(responseContent);

        return roomResponse?.RoomId;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while creating the room: {ex.Message}");
        return null;
    }
}

async Task ReceiveMessagesAsync(ClientWebSocket webSocket)
{
    messages = new List<Message>();
    var buffer = new byte[1024 * 4];
    while (webSocket.State == WebSocketState.Open)
    {
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
        else
        {
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var receivedMessage = JsonSerializer.Deserialize<Message>(message);

            if (receivedMessage is not null)
            {
                messages.Add(receivedMessage);
            }

            PrintMessages(messages);
        }
    }
}
#endregion