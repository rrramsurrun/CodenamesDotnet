using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Websocket
{
  public class SocketMessage
  {
    public string header { get; set; }
    public Dictionary<string, string> body { get; set; }
    public SocketMessage(string header, Dictionary<string, string> body)
    {
      this.header = header;
      this.body = body;
    }
  }
}