using Microsoft.AspNetCore.Mvc;
using PlaXBackEnd.Models;
using PlaXBackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CS8604 // Possible null reference argument.
// Need to add profile picture upload and changing.
namespace PlaXBackEnd.Controllers
{
  [ApiController]
  [Authorize]
  [Route("[controller]")]
  public class UserController : ControllerBase
  {
    private readonly UserService _userService;
    private readonly IAmazonS3 _s3Client;
    private readonly CliqueService _cliqueService;
    private readonly ListingService _listingService;
    public UserController(UserService userService, CliqueService cliqueService, IAmazonS3 s3Client, ListingService postingService)
    {
      _userService = userService;
      _s3Client = s3Client; 
      _listingService = postingService;
      _cliqueService = cliqueService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetPresignedURLProfilePic()
    {
      
      String username =  getUserNameFromJWT((ClaimsIdentity)User.Identity);
      string urlString = "";
      try
      {
        GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
        {
          BucketName = "manilovefrogs",
          Key = username,
          Verb = HttpVerb.PUT,
          Expires = DateTime.UtcNow.AddMinutes(15) ,// Give 15 minutes for the preson to upload
        };
        urlString = _s3Client.GetPreSignedURL(request1);
      }
      catch (AmazonS3Exception e)
      {
        Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
      }
      catch (Exception e)
      {
        Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
      }
      User theUser = await _userService.GetByUsername(username);
      // https://s3-ap-southeast-2.amazonaws.com/{bucket}/{key}
      if (theUser != null)
      {
        theUser.ProfilePicture = "https://manilovefrogs.s3.us-west-1.amazonaws.com/" + username;
        await _userService.UpdateAsync(theUser.Id, theUser);
      }
      return Ok(urlString);
    }

    // This method gets a list of all the users.
    [Route("[action]")]
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<List<User>> Get() =>
        await _userService.GetAsync();
    // Gets a specific user, admin stuff only
    [HttpGet("{id:length(24)}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<User>> GetByID(string id)
    {
      var user = await _userService.GetAsync(id);

      if (user is null)
      {
        return NotFound();
      }

      return user;
    }
    [HttpGet("{username}/profile")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> GetByUserName(string username)
    {
      var user = await _userService.GetByUsername(username);

      if (user is null)
      {
        return NotFound();
      }
      user.Password = null;
      user.Badges = null;
      user.Messages = null;
      return user;
    }
    [Route("[action]")]
    [HttpPost]
    [Authorize(Roles = "Admin")]
    // Create a new user with out the sign in process, might be usefull to give clubs and stuff sign in accounts to try the service out. 
    public async Task<IActionResult> Post(User newUser)
    {
      await _userService.CreateAsync(newUser);

      return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }

    // This is for when a user wants to update there information.
    [HttpPut("fishy/Update")]
    public async Task<IActionResult> Update(User updatedUser) 
    {
      // User info comes from the bearer token they have been given on login.
       var username = getUserNameFromJWT((ClaimsIdentity)User.Identity);

      if (username == null || updatedUser == null)
        return NotFound();
      // First we get the user in the database, this is given by the JWT TOKEN
      User theUser = await _userService.GetByUsername(username);
      if (theUser is null)
      {
        return BadRequest();
      }
      // There probably is some sort of syntax to take care of this but for now this works. can do the inverse as well we will see if we have more things a user can change
      theUser.Bio = updatedUser.Bio;
      await _userService.UpdateAsync(theUser.Id, theUser);

      return NoContent();
    }
    // This is for when a user wants to update there information.
    [HttpGet("fishy/GetMyInfo")]
    public async Task<ActionResult<User>> GetMyInfo()
    {
      // User info comes from the bearer token they have been given on login.
      var username = getUserNameFromJWT((ClaimsIdentity)User.Identity);

      if (username == null)
        return NotFound();
      // First we get the user in the database, this is given by the JWT TOKEN
      User theUser = await _userService.GetByUsername(username);
      if (theUser is null)
      {
        return BadRequest();
      }

      return theUser;
    }

    // This is for when a user wants to update there information.
    [HttpPut("password/Update")]
    public async Task<IActionResult> passwordUpdate(User updatedUser)
    {
      // User info comes from the bearer token they have been given on login.
      var username = getUserNameFromJWT((ClaimsIdentity)User.Identity);

      if (username == null || updatedUser == null)
        return NotFound();
      // First we get the user in the database, this is given by the JWT TOKEN
      User theUser = await _userService.GetByUsername(username);
      if (theUser is null)
      {
        return BadRequest();
      }
      // There probably is some sort of syntax to take care of this but for now this works. can do the inverse as well we will see if we have more things a user can change
      theUser.Password = SecurePasswordHasher.Hash(updatedUser.Password);
      await _userService.UpdateAsync(theUser.Id, theUser);

      return NoContent();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] User user)
    {
      if (user is null)
      {
        return BadRequest("Invalid client request");
      }
      // Probably need to add some form of hashing here. 
   var theUser = await _userService.GetByUsername(user.Username);
      if (theUser is null)
      {
        return BadRequest();
      }
      String role = "";
      if(SecurePasswordHasher.Verify(user.Password , theUser.Password))
      {
        // Checking to see if we are null here or not
        if (theUser.Role != null)
        {
           role = theUser.Role;
        }
        else
        {
           role = "baseUser";
        }
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345")); // This is changed obviously in production to rely on the secrets file
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var tokeOptions = new JwtSecurityToken(
            
            issuer: "https://localhost:5001",
            audience: "https://localhost:5001",
            claims: new[] {
        new Claim(ClaimTypes.Role, role) , new Claim(ClaimTypes.Name, user.Username)},
            expires: DateTime.Now.AddMinutes(300),
            signingCredentials: signinCredentials
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        return Ok(new AuthenticatedResponse { Token = tokenString });


      }
      return Unauthorized();
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> signupAsync([FromBody] User user)
    {
      if (user is null)
      {
        return BadRequest("Invalid client request");
      }
      var theUser = await _userService.GetByUsername(user.Username); // Check to see if username exsist in db or not.
      if (theUser == null && user.Id == null) // User ID is null to prevent people from giving custom id to input
      {
        user.Role = "baseUser";
        user.Badges = new List<Badge>();
        user.DisplayedBadges = new List<Badge>();
        user.ProfilePicture = "https://preview.redd.it/6xo0vv4v9ov21.jpg?auto=webp&s=7a6327082a56a837d01d51f3f9048699d7e3ed43";
        user.Bio = "User does not have a bio yet";
        user.UserListings = new List<Listing>();
        user.UserCliques = new List<String>();
        user.Password = SecurePasswordHasher.Hash(user.Password);
        user.Messages = new List<String>();
        await _userService.CreateAsync(user);

        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
      }
      return BadRequest("Username taken");
    }

    [HttpDelete("{id:length(24)}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
      var user = await _userService.GetAsync(id);

      if (user is null)
      {
        return NotFound();
      }
      var deleteRequest = new DeleteObjectRequest
      {

        Key = user.Username,
        BucketName = "manilovefrogs",

      };
      // This deletes the users profile picture.
      await _s3Client.DeleteObjectAsync(deleteRequest);
      await _userService.RemoveAsync(id);

      return NoContent();
    }

    [HttpPut("{username}/GiveBadge")]
    [Authorize(Roles = "Admin,Club")]
    public async Task<IActionResult> GiveBadge(string username, Badge badge)
    {
      // First we get the user in the database.
      User theUser = await _userService.GetByUsername(username);
      if (theUser is null)
      {
        return BadRequest();
      }
      // then we update the user.
      if(theUser.Badges == null) // We create badges for the user here
      {
        theUser.Badges = new List<Badge>();
      }
      theUser.Badges.Add(badge);
#pragma warning disable CS8604 // Null check above.
      await _userService.UpdateAsync(theUser.Id, theUser);

      return CreatedAtAction(nameof(GiveBadge), theUser);
    }

    // Gives a user a role.
    [HttpPut("{username}/GiveRole")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GiveRole(string username, Role userRole)
    {
      // First we get the user in the database.
      var theUser = await _userService.GetByUsername(username);
      if (theUser is null)
      {
        return BadRequest();
      }
      // then we update the user.
      theUser.Role = userRole.role;
#pragma warning disable CS8604 // Null check above.
      await _userService.UpdateAsync(theUser.Id, theUser);

      return CreatedAtAction(nameof(GiveRole), theUser);
    }

    [HttpPut("set/SetBadges")]
    public async Task<IActionResult> SetBadges([FromBody] List<Badge> badges)
    {  
      var username = getUserNameFromJWT((ClaimsIdentity)User.Identity);
      if (badges == null || username == null)
        return NotFound();
      // First we get the user in the database, this is given by the JWT TOKEN
      User theUser = await _userService.GetByUsername(username);
      if (theUser is null)
      {
        return BadRequest();
      }
      if(theUser.DisplayedBadges == null) // Check if null here
      {
        theUser.DisplayedBadges = new List<Badge>();
      }
      theUser.DisplayedBadges.Clear(); // Clear the users displayed badges. 
      foreach (Badge badge in badges)
      {
        if(theUser.Badges.Any(b => b.BadgeName == badge.BadgeName))
        {
          theUser.DisplayedBadges.Add(badge);
        }
        
      }
      await _userService.UpdateAsync(theUser.Id, theUser);
      return CreatedAtAction(nameof(SetBadges), theUser); 
    }


    [HttpDelete("{listingID}/DeleteListing")]
    public async Task<IActionResult> DeleteListing(string listingID)
    {
      var username = getUserNameFromJWT((ClaimsIdentity)User.Identity);
      if (username == null)
        return NotFound();
      // First we get the user in the database, this is given by the JWT TOKEN
      User theUser = await _userService.GetByUsername(username);
      Listing theListing;
      if (!theUser.Role.Equals("Admin"))
      {
         theListing = theUser.UserListings.DistinctBy(b => b.Id == listingID).FirstOrDefault(); // Here we get the listing
        if (theListing == null)
        {
          return BadRequest("Listing not found");
        }
        // Remove listing from the user
        theUser.UserListings.Remove(theListing);
      }
      else
      {
         theListing = await _listingService.GetAsync(listingID);
        if (theListing == null)
        {
          return BadRequest("Listing not found");
        }
        // Remove listing from the user
        User theSeller = await _userService.GetByUsername(theListing.SellerUserName);
        Listing theSellerListing = theSeller.UserListings.DistinctBy(b => b.Id == listingID).FirstOrDefault();
        theSeller.UserListings.Remove(theSellerListing); 
      }
      
 

      // Remove the listing from its clique
      Clique theClique = await _cliqueService.GetByUsername(theListing.Clique);
     Listing theCliqueListing = theClique.Listings.DistinctBy(b => b.Id == listingID).FirstOrDefault(); // I dont think we can do a simple remove here(pointers), might be wrong but I know this works
      if( theCliqueListing != null)
      {// We ask if we can remove the item from the listing if the clique has not removed it.

        theClique.Listings.Remove(theCliqueListing); // remove the listing from the clique
      }

      await _cliqueService.UpdateAsync(theClique.Id, theClique);
      await _userService.UpdateAsync(theUser.Id, theUser);
      await _listingService.RemoveAsync(listingID);

      // Next we delete the image of from S3.

      var deleteRequest = new DeleteObjectRequest
      {

        Key = theListing.ImageKey,
        BucketName = "manilovefrogs",

      };
      await _s3Client.DeleteObjectAsync(deleteRequest);
      return Ok("Posting deleted.");

    }


    private String getUserNameFromJWT(ClaimsIdentity? theIdentity)
    {
      var identity = theIdentity;
      if (identity == null)
        return null;
      IEnumerable<Claim> claims = identity.Claims;
      return claims?.FirstOrDefault(x => x.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", StringComparison.OrdinalIgnoreCase))?.Value;
    }

    
  }
}
