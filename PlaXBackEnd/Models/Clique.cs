using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PlaXBackEnd.Models
{
  /**
   * This class represnt a Clique in which posting and message boards are found.
   * 
   */
  public class Clique
  {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("Name")]
    public string? Name { get; set; }
    [BsonElement("Listings")]
    public IList<Listing>? Listings { get; set; } // Listing that are in the clique.
    [BsonElement("Postings")]
    public IList<Posting>?  Postings { get; set; } // These are the posting in the cliqe
    [BsonElement("PinnedPostings")]
    public IList<Posting>? PinnedPostings { get; set; } // These are the pinned posting in the clique

    [BsonElement("Catagories")]
    public Catagories? Catagories { get; set; } // These are the catagories determined by the clique leaders.
    [BsonElement("ApprovedUsers")]
    public IList<String>? ApprovedUsers {get; set; } // User that are allow to post!

    [BsonElement("RequestedUsers")]
    public IList<String>? RequestedUsers { get; set; } //  Users that want to post!

    [BsonElement("Mods")]
    public IList<String>? Mods { get; set; } // The user that set everything , and can remove stuff with will.
    [BsonElement("Bio")]
    public String? Bio { get; set; } // The user that set everything , and can remove stuff with will.
    [BsonElement("Badges")]
    public IList<Badge>? Badges { get; set; } // The type of badges that a user can have, make it easier for a clique to give badges
    [BsonElement("ProfilePic")]
    public String? ProfilePic { get; set; }

    [BsonElement("Items")]
    public IList<Item>? Items { get; set; } // These are the items at the auction. 


    [BsonElement("ItemsSold")]
    public IList<Item>? ItemsSold { get; set; } // These are the items that are now sold Items -> ItemSold on sold
  }
}
