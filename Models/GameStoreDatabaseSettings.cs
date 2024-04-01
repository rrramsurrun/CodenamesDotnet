using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codenames.Models
{
  public class GameStoreDatabaseSettings
  {
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string GamesCollectionName { get; set; } = null!;
  }
}