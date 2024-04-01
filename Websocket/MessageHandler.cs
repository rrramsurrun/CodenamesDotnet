using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Codenames.Websocket
{
  public class MessageHandler
  {
    private ISocketHandler socketHandler;

    private Dictionary<string, string[]> _requestKeys = new Dictionary<string, string[]>(){
      {"Join Room",["room"]}
    };

    public MessageHandler(ISocketHandler codenamesRepo)
    {
      socketHandler = codenamesRepo;
    }

    public async Task AddSocket(WebSocket ws)
    {
      socketHandler.AddSocket(ws);
      await Echo(ws);
    }

    public async Task HandleMessage(WebSocket ws, SocketMessage msg)
    {
      var check = CheckValidRequest(ws, msg);
      if (check != "")
      {
        await socketHandler.Emit(ws, ErrorMessage(check));
      }

      if (msg.requestType == "Join Room")
      {
        socketHandler.JoinRoom(ws, msg.body["room"]);
      }
      else
      {
        await socketHandler.Broadcast(ws, msg);
      }
    }

    private string CheckValidRequest(WebSocket ws, SocketMessage msg)
    {
      if (!_requestKeys.ContainsKey(msg.requestType))
      {
        return "The request type is not valid";
      }
      var keys = _requestKeys[msg.requestType];
      if (!keys.All(key => msg.body.ContainsKey(key)))
      {
        return "A request type of " + msg.requestType + " requires the keys: " + String.Join(',', keys);
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
          if (messageObject is not null)
          {
            await HandleMessage(ws, messageObject);
          }
        }
        catch (Exception e)
        {
          if (e is JsonException)
          {
            await socketHandler.Emit(ws, ErrorMessage("Messages must be in JSON with the keys of 'header' and 'body'."));
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