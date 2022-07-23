using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PlaXBackEnd.Models
{
  public class Catagories
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("Name")]
    public string? Name { get; set; }
    [BsonElement("SubCatagories")]
    public IList<Catagories>? SubCatagories { get; set; }
    

  }
}
