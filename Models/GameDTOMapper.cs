using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Models
{
  public static class GameDTOMapper
  {
    public static GameJoinDTO MapToGameJoinDTO(Game game, int userId)
    {
      return new GameJoinDTO
      {
        GameId = game.Id,
        PlayerCount = game.PlayerCount,
        UserId = userId,
        Role = game.GetUserRole(userId),
        Words = game.Words,
        Codex = game.GetCodex(userId),
        FirstTurn = game.FirstTurn
      };
    }
    public static GameUpdateDTO MapToGameUpdateDTO(Game game)
    {
      return new GameUpdateDTO
      {
        Nicknames = game.Nicknames,
        ResetGameSurvey = game.ResetGameSurvey,
        Clues = game.Clues,
        Turn = game.Turn,
        Win = game.Win,
        Revealed = game.GetRevealed()
      };
    }

    public static GameDetailsDTO MapToGameDetailsDTO(Game game)
    {
      return new GameDetailsDTO
      {
        Nicknames = game.Nicknames,
        GameId = game.Id,
        PlayerCount = game.PlayerCount,
      };
    }
    public static GameResetDTO MapToGameResetDTO(Game game, int userId)
    {
      return new GameResetDTO
      {
        Words = game.Words,
        Codex = game.GetCodex(userId),
        FirstTurn = game.FirstTurn
      };
    }
  }
}