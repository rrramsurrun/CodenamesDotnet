using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Models
{
  public class Clue
  {
    public int clueGiverIndex;
    public string clueWord = "";
    public int clueWordCount;
    public List<string> clueGuesses = [];

    public Clue(int clueGiverIndex, string clueWord, int clueWordCount)
    {
      this.clueGiverIndex = clueGiverIndex;
      this.clueWord = clueWord;
      this.clueWordCount = clueWordCount;
    }
  }
}