using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codenames.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Codenames.Services
{
  public class GameService
  {
    private readonly IMongoCollection<Game> _gameCollection;
    public GameService(IOptions<GameStoreDatabaseSettings> gameStoreDatabaseSettings)
    {
      var mongoClient = new MongoClient(
        gameStoreDatabaseSettings.Value.ConnectionString);
      var mongoDatabase = mongoClient.GetDatabase(gameStoreDatabaseSettings.Value.DatabaseName);
      _gameCollection = mongoDatabase.GetCollection<Game>(gameStoreDatabaseSettings.Value.GamesCollectionName);
    }
    public async Task<Game?> GetAsync(string id) =>
    await _gameCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Game newGame)
    {
      await _gameCollection.InsertOneAsync(newGame);
    }

    public async Task UpdateAsync(string id, Game updatedGame) =>
        await _gameCollection.ReplaceOneAsync(x => x.Id == id, updatedGame);
  }
}