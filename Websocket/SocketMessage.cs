using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Websocket
{
  public class SocketMessage
  {
    public string requestType { get; set; }
    public Dictionary<string, string> body { get; set; }
    public SocketMessage(string requestType, Dictionary<string, string> body)
    {
      this.requestType = requestType;
      this.body = body;
    }
  }
}