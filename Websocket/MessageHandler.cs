using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Codenames.Models;
using Codenames.Services;

namespace Codenames.Websocket
{
  public class MessageHandler
  {
    private SocketHandler socketHandler;
    private GameManager _gameManager;

    private Dictionary<string, string[][]> _requestKeys = new(){
      {"newGame",[["role","nickname","playercount"],["string","string","integer"]]},
      {"joinGame",[["role","nickname","gameId"],["string","string","string"]]},
      {"rejoinGame",[["userId","gameId"],["string","string"]]},
      {"findGame",[["gameId"],["string"]]}
    };

    public MessageHandler(SocketHandler codenamesRepo, GameManager gameManager)
    {
      socketHandler = codenamesRepo;
      _gameManager = gameManager;
    }

    public async Task AddSocket(WebSocket ws)
    {
      socketHandler.AddSocket(ws);
      await Echo(ws);
    }

    public async Task HandleMessage(WebSocket ws, SocketInMessage msg)
    {
      var InvalidRequestMessage = CheckValidRequest(ws, msg);
      if (InvalidRequestMessage != "")
      {
        await socketHandler.Emit(ws, ErrorMessage(InvalidRequestMessage));
        return;
      }

      switch (msg.requestType)
      {
        case "newGame":
          await NewGame(ws, msg.body["role"], msg.body["nickname"], msg.body["playercount"]);
          break;
        case "joinGame":
          await FindAndJoinGame(ws, msg.body["role"], msg.body["nickname"], msg.body["gameId"]);
          break;
        case "rejoinGame":
          await RejoinGame(ws, msg.body["userId"], msg.body["gameId"]);
          break;

      }
    }

    private async Task NewGame(WebSocket ws, string role, string nickname, string playerCount)
    {
      var game = await _gameManager.NewGame(int.Parse(playerCount));
      await JoinGame(ws, role, nickname, game);
    }

    private async Task FindAndJoinGame(WebSocket ws, string role, string nickname, string gameId)
    {
      var game = await _gameManager.LoadGame(gameId);
      if (game is null)
      {
        await socketHandler.Emit(ws, ErrorMessage("Could not find a game with that ID"));
        return;
      }
      await JoinGame(ws, role, nickname, game);
    }
    private async Task RejoinGame(WebSocket ws, string userId, string gameId)
    {
      var game = await _gameManager.LoadGame(gameId);
      if (game is null)
      {
        await socketHandler.Emit(ws, ErrorMessage("Could not find a game with that ID"));
        return;
      }
      if (!game.OverwriteUser(int.Parse(userId), ws.GetHashCode()))
      {
        await socketHandler.Emit(ws, ErrorMessage("Could not find a game with that User ID"));
        return;
      }

      var updatedGame = await _gameManager.UpdateGame(game);
      await socketHandler.SendJoinData(ws, updatedGame);
      await socketHandler.SendUpdateData(ws, game);
    }

    private async Task JoinGame(WebSocket ws, string role, string nickname, Game game)
    {
      //Move this to the 'game' class

      if (!game.SetUser(role, ws.GetHashCode(), nickname))
      {
        await socketHandler.Emit(ws, ErrorMessage("Could not set user"));
      }
      //
      var updatedGame = await _gameManager.UpdateGame(game);
      await socketHandler.SendJoinData(ws, updatedGame);
      await socketHandler.BroadcastUpdateData(game);
    }

    private string CheckValidRequest(WebSocket ws, SocketInMessage msg)
    {
      if (!_requestKeys.ContainsKey(msg.requestType))
      {
        return "The request type is not valid";
      }
      var keys = _requestKeys[msg.requestType][0];
      if (keys.Length == 0) return "";
      if (!keys.All(key => msg.body.ContainsKey(key)))
      {
        return "A request type of " + msg.requestType + " requires the keys: " + string.Join(',', keys);
      }
      var keyTypes = _requestKeys[msg.requestType][1];
      for (int i = 0; i < keyTypes.Length; i++)
      {
        var val = msg.body[keys[i]];
        if (val == "") return "The key \"" + keys[i] + "\" must be populated with a value";
        if (keyTypes[i] == "integer")
        {
          try
          {
            var intTest = int.Parse(val);
          }
          catch
          {
            return "The key \"" + keys[i] + "\" must be populated with an integer";
          }
        }
      }
      return "";
    }

    private async Task Echo(WebSocket ws)
    {
      while (ws.State == WebSocketState.Open)
      {
        var buffer = new ArraySegment<byte>(new byte[4096]);
        var received = await ws.ReceiveAsync(buffer, CancellationToken.None);
        var bufferArray = buffer.Array ?? [];
        var message = Encoding.UTF8.GetString(bufferArray, buffer.Offset, buffer.Count);
        message = message.Trim('\0');
        try
        {
          var messageObject = JsonSerializer.Deserialize<SocketInMessage>(message);
          if (messageObject is not null && messageObject.requestType is not null && messageObject.body is not null)
          {
            await HandleMessage(ws, messageObject);
          }
          else
          {
            await socketHandler.Emit(ws, ErrorMessage("Messages must be in JSON with the keys of \'header\' and \'body\'."));
          }
        }
        catch (Exception e)
        {
          if (e is JsonException)
          {
            await socketHandler.Emit(ws, ErrorMessage("Messages must be in JSON with the keys of \'header\' and \'body\'."));
          }
          else
          {
            Console.WriteLine(e);
          }
        }
      }
    }
    private static SocketOutMessage ErrorMessage(string msg)
    {
      return new SocketOutMessage(responseType: "Error", new Dictionary<string, string>() { { "Message", msg } });
    }
  }
}