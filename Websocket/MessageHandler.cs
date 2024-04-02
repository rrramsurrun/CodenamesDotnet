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
    private ISocketHandler socketHandler;
    private GameManager _gameManager;

    private Dictionary<string, string[][]> _requestKeys = new(){
      {"newGame",[["role","nickname","playercount"],["string","string","integer"]]}
    };

    public MessageHandler(ISocketHandler codenamesRepo, GameManager gameManager)
    {
      socketHandler = codenamesRepo;
      _gameManager = gameManager;
    }

    public async Task AddSocket(WebSocket ws)
    {
      socketHandler.AddSocket(ws);
      await Echo(ws);
    }

    public async Task HandleMessage(WebSocket ws, SocketMessage msg)
    {
      var InvalidRequestMessage = CheckValidRequest(ws, msg);
      if (InvalidRequestMessage != "")
      {
        await socketHandler.Emit(ws, ErrorMessage(InvalidRequestMessage));
      }


      if (msg.requestType == "newGame")
      {
        await NewGame(ws, msg.body["role"], msg.body["nickname"], msg.body["playercount"]);
      }
      else
      {
        await socketHandler.Broadcast(ws, msg);
      }
    }

    private async Task NewGame(WebSocket ws, string role, string nickname, string playerCount)
    {
      var playerCountInt = Int32.Parse(playerCount);
      var game = await _gameManager.NewGame(Int32.Parse(playerCount));
      await JoinGame(ws, role, nickname, game);
    }

    private async Task JoinGame(WebSocket ws, string role, string nickname, Game game)
    {
      int roleInt;
      switch (role.ToUpper())
      {
        case "RED SPYMASTER":
          roleInt = 0;
          break;
        case "RED OPERATIVE":
          roleInt = 1;
          break;
        case "BLUE SPYMASTER":
          roleInt = 2;
          break;
        case "BLUE OPERATIVE":
          roleInt = 3;
          break;
        default:
          //Shouldn't happen
          roleInt = 4;
          break;
      }
      if (roleInt == 4)
      {
        await socketHandler.Emit(ws, ErrorMessage("Could not set user"));
        return;
      }
      if (!game.SetUser(roleInt, ws.GetHashCode(), nickname))
      {
        await socketHandler.Emit(ws, ErrorMessage("Could not set user"));
      }
      var updatedGame = await _gameManager.UpdateGame(game);
      await socketHandler.BroadcastGame(updatedGame);
    }

    private string CheckValidRequest(WebSocket ws, SocketMessage msg)
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
            var intTest = Int32.Parse(val);
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
          var messageObject = JsonSerializer.Deserialize<SocketMessage>(message);
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
    private SocketMessage ErrorMessage(string msg)
    {
      return new SocketMessage(requestType: "Error", body: new Dictionary<string, string>() { { "Message", msg } });
    }
  }
}