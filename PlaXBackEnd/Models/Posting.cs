using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PlaXBackEnd.Models
{
  public class Posting
  {
    [BsonId]
    public int Id { get; set; }
    [BsonElement("postTitle")]
    public string? postTitle { get; set; } // The post text itself.
    [BsonElement("postText")]
    public string? postText { get; set; } // The post text itself.
    [BsonElement("comments")]
    public IList<String>? comments  { get; set; } // The comments on said post.
    [BsonElement("Clique")]
    public string? Clique { get; set; } // The clique this post belongs to.
  }
}
