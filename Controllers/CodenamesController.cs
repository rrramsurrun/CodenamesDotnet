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
    private readonly GameManager _gameManager;
    private readonly MessageHandler _messageHandler;
    public CodenamesController(ISocketHandler socketHandler, GameService gameService)
    {
      _gameManager = new GameManager(gameService);
      _messageHandler = new MessageHandler(socketHandler);
    }

    [Route("/codenames")]
    public async Task ManageWebsockets()
    {
      if (HttpContext.WebSockets.IsWebSocketRequest)
      {
        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await _messageHandler.AddSocket(ws);
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
      return Ok(await _gameManager.NewGame());
    }
    [Route("/codenamesnewgame/{id}")]
    [HttpGet]
    public async Task<ActionResult<Game>> GetExistingGame(string id)
    {
      try
      {
        return Ok(await _gameManager.LoadGame(id));
      }
      catch (Exception e)
      {
        return NoContent();
      }
    }

  }
}