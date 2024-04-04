using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codenames.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Codenames.Services
{
  public class GameService
  {
    private readonly IMongoCollection<Game> _gameCollection;
    private readonly IMongoCollection<WordBson> _wordCollection;
    public GameService(IOptions<GameStoreDatabaseSettings> gameStoreDatabaseSettings)
    {
      var mongoClient = new MongoClient(
        gameStoreDatabaseSettings.Value.ConnectionString);
      var mongoDatabase = mongoClient.GetDatabase(gameStoreDatabaseSettings.Value.DatabaseName);
      _gameCollection = mongoDatabase.GetCollection<Game>(gameStoreDatabaseSettings.Value.GamesCollectionName);
      _wordCollection = mongoDatabase.GetCollection<WordBson>(gameStoreDatabaseSettings.Value.WordsCollectionName);
    }
    public async Task<Game?> GetAsync(string id) =>
    await _gameCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Game?> GetAsyncByUserId(int userId) =>
    await _gameCollection.Find(x => x.UserIds.Contains(userId)).FirstOrDefaultAsync();

    public async Task<Game> CreateAsync(Game newGame)
    {
      await _gameCollection.InsertOneAsync(newGame);
      return newGame;
    }

    public async Task UpdateAsync(string id, Game updatedGame) =>
        await _gameCollection.ReplaceOneAsync(x => x.Id == id, updatedGame);

    public async Task<List<string>> GetGameWords()
    {
      var wordBsons = await _wordCollection.AsQueryable().Sample(25).ToListAsync();
      return wordBsons.ConvertAll<string>(x => (string)x.Word);
    }
  }
}