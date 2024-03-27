using System.Net.WebSockets;

namespace Codenames.Websocket
{
  public interface ISocketHandler
  {
    Task Emit(WebSocket ws, SocketMessage messageObject);
    Task Broadcast(WebSocket ws, SocketMessage messageObject);
    void AddSocket(WebSocket ws);
    void RemoveSocket(WebSocket ws);
    void JoinRoom(WebSocket ws, string room);
  }
}