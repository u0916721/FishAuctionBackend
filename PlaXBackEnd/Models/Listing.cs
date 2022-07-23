using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace PlaXBackEnd.Models
{
  /**
   * This class represent a posting in the database and all its informaiton. 
   * 
   */
  public class Listing
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Title")]
    public string Title { get; set; } = null!;

    [BsonElement("SellerUserName")]
    public string SellerUserName { get; set; } = null!;

    [BsonElement("Pickup")]
    public bool Pickup { get; set; }

    [BsonElement("Ships")]
    public bool Ships { get; set; }

    [BsonElement("Description")]
    public string? Description { get; set; } = null!;

    [BsonElement("ImageLink")]
    public string? ImageLink { get; set; } //   The aws link

    [BsonElement("City")]
    public string City { get; set; } = null!;

    [BsonElement("State")]
    public string State { get; set; } = null!;

    [BsonElement("Catagories")]
    public IList<string>? Catagories { get; set; } = null!;

    [BsonElement("Lat")]
    public double Lat { get; set; }

    [BsonElement("Long")]
    public double Long { get; set; }

    [BsonElement("Price")]
    public int Price { get; set; }


    [BsonElement("Clique")]
    public string? Clique { get; set; } // The clique that this post belongs to.


    [BsonElement("ImageKey")]
    public string? ImageKey { get; set; } // The clique that this post belongs to.


    [BsonElement("UserBadges")]
    public IList<string>? UserBadges { get; set; }

  }
}
