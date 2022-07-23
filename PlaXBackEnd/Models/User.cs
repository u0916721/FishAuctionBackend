using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PlaXBackEnd.Models
{
  /*
   *This class repersent a user. 
   * 
   */
  public class User
  {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("Username")]
    public string Username { get; set; } = null!;
    [BsonElement("Password")]
    public string Password { get; set; } = null!;
    [BsonElement("Role")] // Roles are user(standard nothing special) , Entity(Can assign badges to users) , and Admin(has all powers) 
    public string? Role { get; set; }
    [BsonElement("Badges")] // Badges that the users have, base users can not give themselves badges
    public IList<Badge>? Badges { get; set; }
    [BsonElement("DisplayedBadges")] // Badges that the user wishes to display, user can choose to diplay any of their badges. 
    public IList<Badge>? DisplayedBadges { get; set; }
    [BsonElement("ProfilePicture")] // Roles are user(standard nothing special) , Entity(Can assign badges to users) , and Admin(has all powers) 
    public string? ProfilePicture { get; set; }
    [BsonElement("Bio")] // This is the users bio that others can see. 
    public string? Bio { get; set; }
    [BsonElement("UserPostings")] // List of users postings.
    public IList<Listing>? UserListings { get; set; }
    [BsonElement("UserCliques")] // List of users postings.
    public IList<String>? UserCliques { get; set; } // The names of all the cliques that the user likes.

    [BsonElement("Messages")] // List of users postings.
    public IList<String>? Messages { get; set; } // All the messagetags that the user has. IE conversations

  }
}
