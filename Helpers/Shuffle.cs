using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Helpers
{
  public static class Shuffle
  {
    public static void ShuffleList<T>(this IList<T> list)
    {
      Random rnd = new Random();
      int n = list.Count;
      while (n > 1)
      {
        n--;
        int k = rnd.Next(n + 1);
        T value = list[k];
        list[k] = list[n];
        list[n] = value;
      }
    }
  }
}