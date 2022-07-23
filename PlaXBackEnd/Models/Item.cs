using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PlaXBackEnd.Models
{
  // This class repersent an item to be seen in the auction
  public class Item
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("ItemName")]
    public string ItemName { get; set; } = null!; // The name of the item
    [BsonElement("Fish")]
    public String Fish { get; set; } = null!; // The fish that is being sold at the moment. acts as a key to the fish db 
    [BsonElement("SoldFor")]
    public int? SoldFor { get; set; } // What the item Sold At.
    [BsonElement("Seller")]
    public string? Seller { get; set; } // Who the seller is.
    [BsonElement("Buyer")]
    public string? Buyer { get; set; } // Who the buyer is.
    [BsonElement("ImageLink")]
    public string? ImageLink { get; set; } // The link to the image for said item

    [BsonElement("Description")]
    public string? Description { get; set; } // A discription of the item
  }
}

