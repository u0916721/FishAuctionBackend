using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PlaXBackEnd.Models
{
  public class Badge
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("BadgeName")]
    public string BadgeName { get; set; } = null!; 
    [BsonElement("CliqueName")]
    public string CliqueName { get; set; } = null!;
    [BsonElement("Descritption")]
    public string? Descritption { get; set; }
    [BsonElement("Color")]
    public string? Color { get; set; } // For css purposes to add customizatoin
    [BsonElement("Font")]
    public string? Font { get; set; } // For css purposes to add customizatoin
  }
}
