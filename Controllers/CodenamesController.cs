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
using Codenames.Models;
using Codenames.Services;


namespace Codenames.Controller
{

  public class CodenamesController : ControllerBase
  {

    private readonly ISocketHandler _socketHandler;
    private readonly GameService _gameService;
    private readonly MessageHandler messageHandler;
    public CodenamesController(ISocketHandler socketHandler, GameService gameService)
    {
      _socketHandler = socketHandler;
      _gameService = gameService;
      messageHandler = new MessageHandler(_socketHandler);
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

    [Route("/codenamesnewgame")]
    [HttpGet]
    public async Task<ActionResult<Game>> GetNewGame()
    {
      var game = new Game(4);
      await _gameService.CreateAsync(game);
      return Ok(game);
    }
    [Route("/codenamesnewgame/{id}")]
    [HttpGet]
    public async Task<ActionResult<Game>> GetExistingGame(string id)
    {
      return await _gameService.GetAsync(id);
    }

  }
}