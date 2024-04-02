using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Codenames.Models;

namespace Codenames.Websocket
{
  public class SocketHandler : ISocketHandler
  {
    static List<WebSocket> connections = [];

    static Dictionary<int, string> rooms = new Dictionary<int, string>();

    public void AddSocket(WebSocket ws)
    {
      connections.Add(ws);
    }
    public void RemoveSocket(WebSocket ws)
    {
      connections.Remove(ws);
      rooms.Remove(ws.GetHashCode());
    }

    public async Task Broadcast(WebSocket ws, SocketMessage messageObject)
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
        if (hashcodes.Contains(socket.GetHashCode())) await Emit(socket, messageObject);
      }
    }
    public async Task BroadcastGame(Game gameObject)
    {

      List<int> hashcodes = gameObject.UserIds;
      //Message all sockets in the same room
      foreach (var socket in connections)
      {
        if (hashcodes.Contains(socket.GetHashCode())) await Emit(socket, gameObject);
      }
    }
    public void JoinRoom(WebSocket ws, string room)
    {
      rooms.Add(ws.GetHashCode(), room);
    }
    public async Task Emit(WebSocket ws, Object messageObject)
    {
      var json = JsonSerializer.Serialize(messageObject);
      var bytes = Encoding.UTF8.GetBytes(json);
      var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
      if (ws.State == WebSocketState.Open)
        await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

    }

  }
}