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
    private ISocketHandler _codenamesRepo;

    public MessageHandler(ISocketHandler codenamesRepo)
    {
      _codenamesRepo = codenamesRepo;
    }

    public async Task AddSocket(WebSocket ws)
    {
      _codenamesRepo.AddSocket(ws);
      await Echo(ws);
    }

    public async Task HandleMessage(WebSocket ws, SocketMessage msg)
    {
      if (msg.header == "Join Room")
      {
        _codenamesRepo.JoinRoom(ws, msg.body["room"]);
      }
      else
      {
        await _codenamesRepo.Broadcast(ws, msg);
      }
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
        Console.WriteLine(message);
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
            await _codenamesRepo.Emit(ws, ErrorMessage("Messages must be in JSON with the keys of 'header' and 'body'."));
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
      return new SocketMessage(header: "Error", body: new Dictionary<string, string>() { { "Message", msg } });
    }
  }
}