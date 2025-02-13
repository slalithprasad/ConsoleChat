using System;
using System.Text.Json.Serialization;

namespace ConsoleApp.Client.Models;

public class Message
{
    [JsonPropertyName("roomId")]
    public string? RoomId { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}