using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Websocket
{
  public class WebSocketMessage
  {
    public string header { get; set; }
    public string body { get; set; }
    public WebSocketMessage(string header, string body)
    {
      this.header = header;
      this.body = body;
    }
  }
}