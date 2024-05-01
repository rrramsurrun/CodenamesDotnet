using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Websocket
{
  public class SocketInMessage
  {
    public string RequestType { get; set; }
    public Dictionary<string, string> Body { get; set; }
    public SocketInMessage(string requestType, Dictionary<string, string> body)
    {
      this.RequestType = requestType;
      this.Body = body;
    }
  }
  public class SocketOutMessage
  {
    public string ResponseType { get; set; }
    public object Body { get; set; }
    public SocketOutMessage(string responseType, object gameData)
    {
      this.ResponseType = responseType;
      this.Body = gameData;
    }
  }
}