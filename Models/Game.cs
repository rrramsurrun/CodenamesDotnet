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
    public List<int> UserIds { get; set; }
    public List<string> Nicknames { get; set; }
    public List<bool> ResetGameSurvey { get; set; }
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
      UserIds = new List<int>(new int[4]);
      Nicknames = new List<string>(new string[4]);
      ResetGameSurvey = new List<bool>(new bool[4]);
    }
    public bool SetUser(int role, int userId, string nickname)
    {
      if (UserIds[role] == 0)
      {
        UserIds[role] = userId;
        Nicknames[role] = nickname;
        return true;
      }
      return false;
    }

  }
}