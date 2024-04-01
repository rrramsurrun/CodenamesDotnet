using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Models
{
  public class Clue
  {
    public int clueGiverNo;
    public string clueWord = "";
    public int clueWordCount;
    public List<string> clueGuesses = [];
  }
}