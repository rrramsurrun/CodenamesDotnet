using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Codenames.Websocket;

namespace Codenames.Controller
{

  public class CodenamesController : ControllerBase
  {

    private readonly ISocketHandler _codenamesRepo;
    private readonly MessageHandler messageHandler;
    public CodenamesController(ISocketHandler codenamesRepo)
    {
      _codenamesRepo = codenamesRepo;
      messageHandler = new MessageHandler(_codenamesRepo);
    }

    [Route("/codenames")]
    public async Task ManageWebsockets()
    {
      if (HttpContext.WebSockets.IsWebSocketRequest)
      {
        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await messageHandler.AddSocket(ws);
      }
      else
      {
        HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      }
    }

  }
}