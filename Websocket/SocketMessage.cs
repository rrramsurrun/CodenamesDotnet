using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Websocket
{
  public class SocketInMessage
  {
    public string requestType { get; set; }
    public Dictionary<string, string> body { get; set; }
    public SocketInMessage(string requestType, Dictionary<string, string> body)
    {
      this.requestType = requestType;
      this.body = body;
    }
  }
  public class SocketOutMessage
  {
    public string ResponseType { get; set; }
    public object body { get; set; }
    public SocketOutMessage(string responseType, object gameData)
    {
      this.ResponseType = responseType;
      this.body = gameData;
    }
  }
}