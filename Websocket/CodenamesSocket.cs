using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Codenames.Websocket
{

  public class CodenamesSocket
  {
    private WebSocket socket { get; set; }
    private string room { get; set; }

    public CodenamesSocket(WebSocket socket, string room)
    {
      this.socket = socket;
      this.room = room;
    }
  }
}
