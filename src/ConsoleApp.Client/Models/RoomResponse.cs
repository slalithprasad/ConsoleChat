using System;
using System.Text.Json.Serialization;

namespace ConsoleApp.Client.Models;

public class RoomResponse
{
    [JsonPropertyName("roomId")]
    public string? RoomId { get; set; }
}