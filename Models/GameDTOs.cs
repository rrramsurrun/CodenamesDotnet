using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Models
{

  public class GameJoinDTO
  {
    public string? GameId { get; set; } = "";
    public string Room { get; set; } = "";
    public int PlayerCount { get; set; }
    public int UserId { get; set; }
    public int Role { get; set; }
    public List<string> Words { get; set; } = [];
    public object? Codex;
    public string FirstTurn { get; set; } = "";
  }

  public class GameUpdateDTO
  {
    public List<string> Nicknames { get; set; } = [];
    public List<bool> ResetGameSurvey { get; set; } = [];
    public List<Clue> Clues { get; set; } = [];
    public int Turn { get; set; }
    public string Win { get; set; } = "";
    public object? Revealed { get; set; }
  }
  public class GameDetailsDTO
  {
    public string? GameId { get; set; } = "";
    public List<string> Nicknames { get; set; } = [];
    public int PlayerCount { get; set; }
  }
}