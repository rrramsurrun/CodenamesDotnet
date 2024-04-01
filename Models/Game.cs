using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Codenames.Models;

namespace Codenames.Models
{

  public class Game
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Room { get; set; } = "";
    public List<string> UserIds { get; set; } = [];
    public List<string> Nicknames { get; set; } = [];
    public int PlayerCount { get; set; }
    public List<string> Revealed { get; set; } = [];
    public List<bool> ResetGameSurvey { get; set; } = [];
    public string Win { get; set; } = "";
    public int Turn { get; set; } = 0;
    public string FirstTurn { get; set; } = "";
    public int TurnNo { get; set; } = 0;
    public List<Clue> Clues { get; set; } = [];
    public List<string> Words { get; set; } = [];
    public Dictionary<string, string> Codex { get; set; } = new Dictionary<string, string>();

    public Game(int playerCount)
    {
      this.PlayerCount = playerCount;
    }

  }
}