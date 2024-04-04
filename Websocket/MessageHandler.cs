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
      {"findGame",[["gameId"],["string"]]},
      {"sendClue",[["userId","clue","clueCount"],["string","string","integer"]]},
      {"clickWord",[["userId","wordIndex"],["string","integer"]]},
      {"endTurn",[["userId"],["string"]]},
      {"leaveGame",[["userId"],["string"]]},
      {"resetGame",[["userId"],["string"]]},
      {"resetGameConfirm",[["userId"],["string"]]},
      {"resetGameReject",[["userId"],["string"]]},
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

    public async Task RouteMessage(WebSocket ws, SocketInMessage msg)
    {
      var InvalidRequestMessage = CheckValidRequest(ws, msg);
      if (InvalidRequestMessage != "")
      {
        await socketHandler.SendErrorMessage(ws, InvalidRequestMessage);
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
        case "findGame":
          await FindGame(ws, msg.body["gameId"]);
          break;
        case "sendClue":
          await AddClue(ws, msg.body["userId"], msg.body["clue"], msg.body["clueCount"]);
          break;
        case "clickWord":
          await ClickWord(ws, msg.body["userId"], msg.body["wordIndex"]);
          break;
        case "endTurn":
          await EndTurn(ws, msg.body["userId"]);
          break;
        case "leaveGame":
          await LeaveGame(ws, msg.body["userId"]);
          break;
        case "resetGame":
          await ResetGame(ws, msg.body["userId"], true);
          break;
        case "resetGameConfirm":
          await ResetGame(ws, msg.body["userId"], false);
          break;
        case "resetGameReject":
          await RejectReset(ws, msg.body["userId"]);
          break;
      }
    }

    private async Task RejectReset(WebSocket ws, string userId)
    {
      int userIdInt = int.Parse(userId);
      var game = await FindGameByUserIdCheckTurn(ws, userIdInt, false);
      if (game is null) return;
      game.NullifyResetSurvey();
      var updatedGame = await _gameManager.UpdateGame(game);
      await socketHandler.BroadcastUpdateData(updatedGame, "rejectResetUpdate");
    }

    private async Task ResetGame(WebSocket ws, string userId, bool firstResetRequest)
    {
      int userIdInt = int.Parse(userId);
      var game = await FindGameByUserIdCheckTurn(ws, userIdInt, false);
      if (game is null) return;
      if (!game.CheckValidResetRequest(userIdInt, firstResetRequest))
      {
        await socketHandler.SendErrorMessage(ws, "Somebody rejected the reset request");
      };
      if (game.ConfirmAllReset())
      {
        var newGame = await _gameManager.RestartGame(game);
        await socketHandler.BroadcastRestartData(newGame);
        await socketHandler.BroadcastUpdateData(newGame);
      }
      else
      {
        var updatedGame = await _gameManager.UpdateGame(game);
        await socketHandler.BroadcastUpdateData(updatedGame);
      }

    }

    private async Task LeaveGame(WebSocket ws, string userId)
    {
      int userIdInt = int.Parse(userId);
      var game = await FindGameByUserIdCheckTurn(ws, userIdInt, false);
      if (game is null) return;
      game.DeleteUser(userIdInt);
      var updatedGame = await _gameManager.UpdateGame(game);
      await socketHandler.BroadcastUpdateData(updatedGame);
      await socketHandler.SendLeaveConfirmation(ws);
    }

    private async Task AddClue(WebSocket ws, string userId, string clue, string clueCount)
    {
      int userIdInt = int.Parse(userId);
      var game = await FindGameByUserIdCheckTurn(ws, userIdInt, true);
      if (game is null) return;
      game.AddClue(userIdInt, clue, int.Parse(clueCount));
      var updatedGame = await _gameManager.UpdateGame(game);
      await socketHandler.SendUpdateData(ws, updatedGame);
    }

    private async Task ClickWord(WebSocket ws, string userId, string wordIndex)
    {
      int userIdInt = int.Parse(userId);
      var game = await FindGameByUserIdCheckTurn(ws, userIdInt, true);
      if (game is null) return;
      game.ClickWord(userIdInt, int.Parse(wordIndex));
      var updatedGame = await _gameManager.UpdateGame(game);
      await socketHandler.SendUpdateData(ws, updatedGame);
    }

    private async Task EndTurn(WebSocket ws, string userId)
    {
      int userIdInt = int.Parse(userId);
      var game = await FindGameByUserIdCheckTurn(ws, userIdInt, true);
      if (game is null) return;
      game.EndGuessing();
      var updatedGame = await _gameManager.UpdateGame(game);
      await socketHandler.SendUpdateData(ws, updatedGame);
    }
    private async Task<Game?> FindGameByUserIdCheckTurn(WebSocket ws, int userId, bool turnSensitive)
    {
      var game = await _gameManager.LoadGameByUserId(userId);
      if (game is null)
      {
        await socketHandler.SendErrorMessage(ws, "Could not find a game using that User ID");
        return null;
      }

      if (turnSensitive && !game.CheckTurn(userId))
      {
        await socketHandler.SendErrorMessage(ws, "It is not your turn");
        return null;
      }
      return game;
    }

    private async Task NewGame(WebSocket ws, string role, string nickname, string playerCount)
    {
      var game = await _gameManager.NewGame(int.Parse(playerCount));
      await JoinGame(ws, role, nickname, game);
    }
    private async Task FindGame(WebSocket ws, string gameId)
    {
      var game = await _gameManager.LoadGame(gameId);
      if (game is null)
      {
        await socketHandler.SendErrorMessage(ws, "Could not find a game with that ID");
        return;
      }
      await socketHandler.SendGameDetails(ws, game);
    }

    private async Task FindAndJoinGame(WebSocket ws, string role, string nickname, string gameId)
    {
      var game = await _gameManager.LoadGame(gameId);
      if (game is null)
      {
        await socketHandler.SendErrorMessage(ws, "Could not find a game with that ID");
        return;
      }
      await JoinGame(ws, role, nickname, game);
    }
    private async Task RejoinGame(WebSocket ws, string userId, string gameId)
    {
      var game = await _gameManager.LoadGame(gameId);
      if (game is null)
      {
        await socketHandler.SendErrorMessage(ws, "Could not find a game with that ID");
        return;
      }
      if (!game.OverwriteUser(int.Parse(userId), ws.GetHashCode()))
      {
        await socketHandler.SendErrorMessage(ws, "Could not find a game with that User ID");
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
        await socketHandler.SendErrorMessage(ws, "The role of " + role + " is taken");
        return;
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
            await RouteMessage(ws, messageObject);
          }
          else
          {
            await socketHandler.SendErrorMessage(ws, "Messages must be in JSON with the keys of \'header\' and \'body\'.");
          }
        }
        catch (Exception e)
        {
          if (e is JsonException)
          {
            await socketHandler.SendErrorMessage(ws, "Messages must be in JSON with the keys of \'header\' and \'body\'.");
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