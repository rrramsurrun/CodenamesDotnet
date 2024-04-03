using System.Net.WebSockets;
using Codenames.Models;

namespace Codenames.Websocket
{
  public interface ISocketHandler
  {
    Task Emit(WebSocket ws, Object messageObject);
    Task Broadcast(WebSocket ws, SocketOutMessage messageObject);
    Task BroadcastGame(GameUpdateDTO gameObject);
    void AddSocket(WebSocket ws);
    void RemoveSocket(WebSocket ws);
    void JoinRoom(WebSocket ws, string room);
  }
}