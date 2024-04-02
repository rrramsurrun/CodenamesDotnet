using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codenames.Models;
using Codenames.Helpers;

namespace Codenames.Services
{
  public class GameManager
  {
    private GameService _gameService;

    public GameManager(GameService gameService)
    {
      _gameService = gameService;
    }

    public async Task<Game> NewGame(int playerCount)
    {
      var game = new Game(playerCount);
      var words = await _gameService.GetGameWords();
      game.Words = words;
      if (game.PlayerCount == 4) game.Codex4Player = Generate4playerCodex(words, game.FirstTurn);
      if (game.PlayerCount == 2) game.Codex2Player = Generate2playerCodex(words);
      await _gameService.CreateAsync(game);
      return game;
    }
    public async Task<Game> UpdateGame(Game game)
    {
      await _gameService.UpdateAsync(game.Id, game);
      return game;
    }
    public async Task<Game?> LoadGame(string id)
    {
      return await _gameService.GetAsync(id);
    }
    private Dictionary<string, string> Generate4playerCodex(List<string> words, string firstTurn)
    {
      List<string> list = new(words);
      Shuffle.ShuffleList(list);
      Dictionary<string, string> dict = [];
      for (int i = 0; i < list.Count; i++)
      {
        if (i == 0)
        {
          dict.Add(list[i], "black");
        }
        else if (i <= 9)
        {
          dict.Add(list[i], firstTurn);
        }
        else if (i <= 17)
        {
          dict.Add(list[i], firstTurn == "red" ? "blue" : "red");
        }
        else
        {
          dict.Add(list[i], "cream");
        }
      }
      return dict;
    }
    private Dictionary<string, string[]> Generate2playerCodex(List<string> words)
    {
      List<string> shuffledWords = new(words);
      Shuffle.ShuffleList(shuffledWords);
      //Many words are different colours between players      
      List<string> player1Colours = ["black",
        "cream",
        "cream",
        "cream",
        "cream",
        "cream",
        "green",
        "green",
        "green",
        "green",
        "green",
        "green",
        "green",
        "green",
        "green",
        "cream",
        "black",
        "cream",
        "cream",
        "cream",
        "cream",
        "cream",
        "cream",
        "cream",
        "black"];
      List<string> player2Colours = ["black",
        "green",
        "green",
        "green",
        "green",
        "green",
        "green",
        "green",
        "green",
        "green",
        "cream",
        "cream",
        "cream",
        "cream",
        "cream",
        "black",
        "black",
        "black",
        "cream",
        "cream",
        "cream",
        "cream",
        "cream",
        "cream",
        "cream",
        "cream"];
      Dictionary<string, string[]> dict = [];
      for (int i = 0; i < shuffledWords.Count; i++)
      {
        dict.Add(shuffledWords[i], [player1Colours[i], player2Colours[i]]);
      }
      return dict;
    }




  }

}