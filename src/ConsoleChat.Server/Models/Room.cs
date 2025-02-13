using System;
using System.Net.WebSockets;
using System.Text.Json.Serialization;

namespace ConsoleChat.Server.Models;

public class Room
{
    [JsonPropertyName("roomId")]
    public string? RoomId { get; set; }

    [JsonPropertyName("clients")]
    public List<WebSocket> Clients { get; set; } = new List<WebSocket>();
}