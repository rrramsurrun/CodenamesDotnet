using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Codenames.Models
{
  public class WordBson
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("word")]
    public required string Word { get; set; }

  }
}