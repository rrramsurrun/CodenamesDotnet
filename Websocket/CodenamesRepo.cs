using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Codenames.Websocket
{
  public class CodenamesRepo : ICodenamesRepo
  {
    static List<WebSocket> connections = [];

    static Dictionary<int, string> rooms = new Dictionary<int, string>();

    public async Task AddSocket(WebSocket ws)
    {
      connections.Add(ws);
      await Echo(ws);
    }

    public async Task Broadcast(WebSocket ws, string header, string message)
    {
      if (!rooms.ContainsKey(ws.GetHashCode()))
      {
        return;
      }
      // Find all sockets in the same room
      var room = rooms[ws.GetHashCode()];
      List<int> hashcodes = [];
      foreach (KeyValuePair<int, string> entry in rooms)
      {
        if (entry.Value == room) hashcodes.Add(entry.Key);
      }
      //Message all sockets in the same room
      foreach (var socket in connections)
      {
        if (hashcodes.Contains(socket.GetHashCode())) await SendMessage(socket, header, message);
      }
    }

    public async Task Emit(WebSocket ws, string header, string message)
    {
      await SendMessage(ws, header, message);
    }
    public async Task JoinRoom(WebSocket ws, string room)
    {
      rooms.Add(ws.GetHashCode(), room);

      await SendMessage(ws, "Joined Room", room);

    }

    public Task RemoveSocket(WebSocket ws)
    {
      throw new NotImplementedException();
    }

    private async Task SendMessage(WebSocket ws, string header, string message)
    {
      var messageObject = new WebSocketMessage(header, message);
      var json = JsonSerializer.Serialize(messageObject);
      var bytes = Encoding.UTF8.GetBytes(json);
      var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
      if (ws.State == WebSocketState.Open)
        await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

    }
    private async Task Echo(WebSocket ws)
    {
      while (ws.State == WebSocketState.Open)
      {
        var buffer = new ArraySegment<byte>(new byte[4096]);
        var received = await ws.ReceiveAsync(buffer, CancellationToken.None);
        var message = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        message = message.Trim('\0');
        try
        {
          var messageObject = JsonSerializer.Deserialize<WebSocketMessage>(message);
          if (messageObject is not null)
          {
            if (messageObject.header == "Join Room")
            {
              await JoinRoom(ws, messageObject.body);
            }
            else
            {
              await Broadcast(ws, messageObject.header, messageObject.body);
            }
          }
        }
        catch (Exception e)
        {
          if (e is JsonException)
          {
            await Emit(ws, "Error", "JSON messages must have a header and a body");
          }
          else
          {
            Console.WriteLine(e);
          }
        }
      }

    }
  }
}