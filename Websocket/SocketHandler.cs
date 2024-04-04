using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Codenames.Models;
using Newtonsoft.Json;

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
    public async Task BroadcastUpdateData(Game game, string header = "gameUpdate")
    {
      GameUpdateDTO gameUpdate = GameDTOMapper.MapToGameUpdateDTO(game);
      SocketOutMessage updateGame = new("gameUpdate", gameUpdate);
      List<int> hashcodes = game.UserIds;
      //Message all sockets in the same room
      foreach (var socket in connections)
      {
        if (hashcodes.Contains(socket.GetHashCode())) await Emit(socket, updateGame);
      }
    }
    public async Task BroadcastRestartData(Game game)
    {
      List<int> hashcodes = game.UserIds;
      //Message all sockets in the same room
      foreach (var socket in connections)
      {
        if (hashcodes.Contains(socket.GetHashCode()))
        {
          GameResetDTO resetGame = GameDTOMapper.MapToGameResetDTO(game, socket.GetHashCode());
          SocketOutMessage updateGame = new("restartGame", resetGame);
          await Emit(socket, updateGame);
        }
      }
    }
    public async Task SendUpdateData(WebSocket ws, Game game)
    {
      GameUpdateDTO gameUpdate = GameDTOMapper.MapToGameUpdateDTO(game);
      SocketOutMessage msg = new("gameUpdate", gameUpdate);
      await Emit(ws, msg);
    }
    public async Task SendGameDetails(WebSocket ws, Game game)
    {
      GameDetailsDTO gameDetails = GameDTOMapper.MapToGameDetailsDTO(game);
      SocketOutMessage preGame = new("findGameResponse", gameDetails);
      await Emit(ws, preGame);
    }
    public async Task SendLeaveConfirmation(WebSocket ws)
    {
      SocketOutMessage leaveGame = new("leaveGameResponse", new Dictionary<string, int>() { { "userId", ws.GetHashCode() } });
      await Emit(ws, leaveGame);
    }
    public async Task SendErrorMessage(WebSocket ws, string msg)
    {
      await Emit(ws, ErrorMessage(msg));
    }
    public async Task Emit(WebSocket ws, SocketOutMessage msg)
    {
      var json = JsonConvert.SerializeObject(msg);
      var bytes = Encoding.UTF8.GetBytes(json);
      var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
      if (ws.State == WebSocketState.Open)
        await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
    }
    private static SocketOutMessage ErrorMessage(string msg)
    {
      return new SocketOutMessage(responseType: "Error", new Dictionary<string, string>() { { "Message", msg } });
    }

  }
}