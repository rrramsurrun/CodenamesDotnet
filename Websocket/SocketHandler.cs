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
  public class SocketHandler
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
    public async Task SendJoinData(WebSocket ws, Game game)
    {
      GameJoinDTO gameJoin = GameDTOMapper.MapToGameJoinDTO(game, ws.GetHashCode());
      SocketOutMessage joinGame = new("joinGameResponse", gameJoin);
      await Emit(ws, joinGame);
    }
    public async Task BroadcastUpdateData(Game game)
    {
      GameUpdateDTO gameUpdate = GameDTOMapper.MapToGameUpdateDTO(game);
      SocketOutMessage updateGame = new("joinGameResponse", gameUpdate);
      List<int> hashcodes = game.UserIds;
      //Message all sockets in the same room
      foreach (var socket in connections)
      {
        if (hashcodes.Contains(socket.GetHashCode())) await Emit(socket, updateGame);
      }
    }
    public async Task SendUpdateData(WebSocket ws, Game game)
    {
      GameUpdateDTO gameUpdate = GameDTOMapper.MapToGameUpdateDTO(game);
      SocketOutMessage updateGame = new("gameUpdate", gameUpdate);
      await Emit(ws, updateGame);
    }
    // public async Task BroadcastGame(Game gameObject)
    // {

    //   List<int> hashcodes = gameObject.UserIds;
    //   //Message all sockets in the same room
    //   foreach (var socket in connections)
    //   {
    //     if (hashcodes.Contains(socket.GetHashCode())) await Emit(socket, gameObject);
    //   }
    // }
    public async Task Emit(WebSocket ws, SocketOutMessage msg)
    {
      var json = JsonSerializer.Serialize(msg);
      var bytes = Encoding.UTF8.GetBytes(json);
      var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
      if (ws.State == WebSocketState.Open)
        await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
    }

  }
}