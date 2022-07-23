using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PlaXBackEnd.Models
{
  //This model reperesent a fish, this is a general fish database. 
  public class Fish
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Name")]
    public string? Name { get; set; } // This is the common name of the fish
    
    [BsonElement("CareLink")]
    public string? CareLink { get; set; } // Link for general care

    [BsonElement("Picture")]
    public List<string>? Pictures { get; set; } // This is a picture of the fish


    [BsonElement("Size")]
    public string? Size { get; set; } // Denotes the size of the fish
    [BsonElement("WaterConditions")]
    public string? WaterConditions { get; set; } // Denotes the ph, temp etc for the fish

  }
}
