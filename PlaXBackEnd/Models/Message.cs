using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PlaXBackEnd.Models
{
  /**
   * This class represnt a Clique in which posting and message boards are found.
   * 
   */
  public class Message
  {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("Tag")]
    public string? Tag { get; set; } // Function as the ID pretty much, clever way to get the said message and verify that correct users have accsess 
    [BsonElement("Messages")]
    public IList<String>? Messages { get; set; } // Messages between the user

  }
}