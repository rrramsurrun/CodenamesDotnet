using System.Net.WebSockets;
using Codenames.Models;

namespace Codenames.Websocket
{
  public interface ISocketHandler
  {
    Task Emit(WebSocket ws, Object messageObject);
    Task Broadcast(WebSocket ws, SocketMessage messageObject);
    Task BroadcastGame(Game gameObject);
    void AddSocket(WebSocket ws);
    void RemoveSocket(WebSocket ws);
    void JoinRoom(WebSocket ws, string room);
  }
}