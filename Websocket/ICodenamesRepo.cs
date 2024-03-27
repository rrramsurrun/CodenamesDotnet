using System.Net.WebSockets;

namespace Codenames.Websocket
{
  public interface ICodenamesRepo
  {
    Task Emit(WebSocket ws, string header, string message);
    Task Broadcast(WebSocket ws, string header, string message);
    Task AddSocket(WebSocket ws);
    Task RemoveSocket(WebSocket ws);
    Task JoinRoom(WebSocket ws, string room);
  }
}