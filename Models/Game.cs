using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Codenames.Models;

namespace Codenames.Models
{

  public class Game
  {
    //Properties that do not change from first game
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public int PlayerCount { get; set; }
    //User details and votes
    public List<int> UserIds { get; set; }
    public List<string> Nicknames { get; set; }
    public List<bool> ResetGameSurvey { get; set; }
    //Properties that change between games
    public List<string> Words { get; set; } = [];
    public string FirstTurn { get; set; }
    public Dictionary<string, string> Codex4Player { get; set; } = [];

    public Dictionary<string, string[]> Codex2Player { get; set; } = [];
    //Properties that hold game state
    public List<string> Revealed4Player { get; set; } = new(new string[25]);
    public List<string[]> Revealed2Player { get; set; } = [];
    public string Win { get; set; } = "";
    public int GuessCount { get; set; } = 0;
    public List<Clue> Clues { get; set; } = [];
    public int Turn { get; set; }

    public Game(int playerCount)
    {
      this.PlayerCount = playerCount;
      Random rnd = new Random();
      Turn = rnd.Next(2) == 0 ? 0 : 2;
      FirstTurn = Turn == 0 ? "red" : "blue";
      UserIds = new List<int>(new int[playerCount]);
      Nicknames = new List<string>(new string[playerCount]);
      ResetGameSurvey = new List<bool>(new bool[playerCount]);
      if (playerCount == 2)
      {//Populate empty revealed array
        for (int i = 0; i < 25; i++) Revealed2Player.Add(new string[2]);
      }
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
    public bool DeleteUser(int userId)
    {
      var i = UserIds.IndexOf(userId);
      if (i == -1) return false;
      UserIds[i] = 0;
      return true;
    }
    public bool CheckTurn(int userId)
    {
      int i = UserIds.IndexOf(userId);
      if (UserIds.IndexOf(userId) != Turn) return false;
      return true;
    }
    public void EndGuessing()
    {
      Turn = (Turn + 1) % PlayerCount;
      GuessCount++;
    }
    public bool ClickWord(int userId, int wordIndex)
    {
      int userIndex = UserIds.IndexOf(userId);
      //Check word exists, then find
      if (wordIndex > 24) return false;
      string word = Words[wordIndex];
      string color = "";
      //4 player logic
      if (PlayerCount == 4)
      {
        color = Codex4Player[word];
        //Add clue to stack
        Clues[GuessCount].clueGuesses.Add(word);
        //Update revealed
        Revealed4Player[userIndex] = color;
        //Check action depending on word color        
        if (color == "black")
        {//clicked assassin card          
          Win = Turn == 1 ? "blue" : "red";
          PopulateRevealed();
        }
        else if (color == "cream"
        || color == "blue" && Turn == 1
        || color == "red" && Turn == 3)
        {//Clicked other team/civilian card          
          EndGuessing();
        }
        //Revealing all of one teams cards results in a win
        if (Win == "")
        {
          CheckWinStatus4Player("blue");
          CheckWinStatus4Player("red");
        }
      }
      //2 player logic
      if (PlayerCount == 2)
      {
        color = Codex2Player[word][userIndex];
        //Add clue to stack
        Clues[GuessCount].clueGuesses.Add(word);
        //Revealed is a mirror image of the codex
        Revealed2Player[wordIndex][Math.Abs(userIndex - 1)] = color;
        //Check action depending on word color
        if (color == "black")
        {//Clicked Assassin card
          Win = "lose";
          PopulateRevealed();
        }
        else if (color == "cream")
        {//Clicked civillian card
          EndGuessing();
        }
        else if (color == "green")
        {//
          CheckWinStatus2Player();
        }
      }

      return true;
    }

    private void PopulateRevealed()
    {//At game end populate the revealed array
      if (PlayerCount == 4)
      {
        for (int i = 0; i < Words.Count; i++)
        {
          Revealed4Player[i] = Codex4Player[Words[i]];
        }
      }
      if (PlayerCount == 2)
      {
        for (int i = 0; i < Words.Count; i++)
        {
          Revealed2Player[i][0] = Codex2Player[Words[i]][0];
          Revealed2Player[i][1] = Codex2Player[Words[i]][1];
        }
      }
    }
    private void CheckWinStatus4Player(string color)
    {
      //First team has 9 cards to uncover, other team has 8
      if (Revealed4Player.Where(x => x.Equals(color)).Count() == (FirstTurn == color ? 9 : 8))
      { //All cards revealed        
        Win = color;
        PopulateRevealed();
      };
    }
    private void CheckWinStatus2Player()
    {
      if (Revealed2Player.Where(x => x[0].Equals("green") || x[1].Equals("green")).Count() == 15)
      {
        Win = "win";
        PopulateRevealed();
      }
    }
    public bool AddClue(int userId, string clue, int clueWordCount)
    {
      int userIndex = UserIds.IndexOf(userId);
      Clues.Add(new Clue(userIndex, clue, clueWordCount));
      Turn++;
      return true;
    }
    public object GetCodex(int userId)
    {
      int i = UserIds.IndexOf(userId);
      if (PlayerCount == 4)
      {
        if (i == 0 || i == 2) { return Codex4Player; }
        else return new Dictionary<string, string>();
      }
      else return Codex2Player.ToDictionary(kvp => kvp.Key, kvp => kvp.Value[i]);
    }
    public object GetRevealed()
    {
      if (PlayerCount == 4) return Revealed4Player;
      return Revealed2Player;
    }
    public int GetUserRole(int userId)
    {
      return UserIds.IndexOf(userId);
    }
    public bool CheckValidResetRequest(int userId, bool firstResetRequest)
    {
      int i = UserIds.IndexOf(userId);
      int resetRequestCount = ResetGameSurvey.Where(x => x.Equals(true)).Count();

      //If this is an initial reset request, the count should be 0
      //If it is a follow up request the count should be greater than 0
      if ((firstResetRequest && resetRequestCount == 0)
      || (!firstResetRequest && resetRequestCount > 0))
      {
        ResetGameSurvey[i] = true;
        return true;
      }
      return false;
    }
    public bool ConfirmAllReset()
    {
      return ResetGameSurvey.Where(x => x.Equals(true)).Count() == UserIds.Where(x => !x.Equals(0)).Count();
    }
    public void NullifyResetSurvey()
    {
      ResetGameSurvey = new List<bool>(new bool[PlayerCount]);
    }
  }
}