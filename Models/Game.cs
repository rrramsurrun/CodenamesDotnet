using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Codenames.Models;
using System.Text.Json.Serialization;

namespace Codenames.Models
{

  public class Game
  {
    //Properties that do not change from first game
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Room { get; set; } = "";
    public int PlayerCount { get; set; }
    //User details and votes
    public List<string> UserIds { get; set; } = [];
    public List<string> Nicknames { get; set; } = [];
    public List<bool> ResetGameSurvey { get; set; } = [];
    //Properties that change between games
    public List<string> Words { get; set; } = [];
    public string FirstTurn { get; set; }
    [JsonIgnore]
    public Dictionary<string, string> Codex4Player { get; set; } = new Dictionary<string, string>();
    [JsonIgnore]
    public Dictionary<string, string[]> Codex2Player { get; set; } = new Dictionary<string, string[]>();
    //Properties that hold game state
    public List<string> Revealed { get; set; } = [];
    public string Win { get; set; } = "";
    public int TurnNo { get; set; } = 0;
    public List<Clue> Clues { get; set; } = [];
    public int Turn { get; set; }
    [BsonIgnore]
    public object Codex { get { if (PlayerCount == 4) { return Codex4Player; } else return Codex2Player; } }
    public Game(int playerCount)
    {
      this.PlayerCount = playerCount;
      Random rnd = new Random();
      Turn = rnd.Next(2) == 0 ? 0 : 2;
      FirstTurn = Turn == 0 ? "red" : "blue";
    }

  }
}