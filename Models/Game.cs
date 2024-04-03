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
      UserIds = new List<int>(new int[playerCount]);
      Nicknames = new List<string>(new string[playerCount]);
      ResetGameSurvey = new List<bool>(new bool[playerCount]);
    }
    public bool SetUser(string roleName, int userId, string nickname)
    {
      int role;
      switch (roleName.ToUpper())
      {
        case "RED SPYMASTER":
          role = 0;
          break;
        case "RED OPERATIVE":
          role = 1;
          break;
        case "BLUE SPYMASTER":
          role = 2;
          break;
        case "BLUE OPERATIVE":
          role = 3;
          break;
        default:
          //Shouldn't happen
          role = 4;
          break;
      }
      if (UserIds[role] == 0 || UserIds[role] == userId)
      {
        UserIds[role] = userId;
        Nicknames[role] = nickname;
        return true;
      }
      return false;
    }

    public bool OverwriteUser(int userId, int newUserId)
    {
      var i = UserIds.IndexOf(userId);
      if (i == -1) return false;
      UserIds[i] = newUserId;
      return true;
    }
    public object GetCodex(int userId)
    {
      var i = UserIds.IndexOf(userId);
      if (PlayerCount == 4)
      {
        if (i == 0 || i == 2) { return Codex4Player; }
        else return new Dictionary<string, string>();
      }
      else return Codex2Player.ToDictionary(kvp => kvp.Key, kvp => kvp.Value[i]);
    }
    public int GetUserRole(int userId)
    {
      return UserIds.IndexOf(userId);
    }

  }
}