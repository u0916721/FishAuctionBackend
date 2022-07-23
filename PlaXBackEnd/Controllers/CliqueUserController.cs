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

namespace PlaXBackEnd.Controllers
{
  [ApiController]
  [Route("User/Cliques")]
  [Authorize]
  public class CliqueUserController : Controller
  {
    private readonly UserService _userService;
    private readonly CliqueService _cliqueService;
    private readonly IAmazonS3 _s3Client;
    private readonly ListingService _listingService;
    public CliqueUserController(UserService userService, CliqueService cliqueService, IAmazonS3 s3Client, ListingService postingService )
    {
      _userService = userService;
      _cliqueService = cliqueService;
      _s3Client = s3Client;
      _listingService = postingService;
    }

    // This method gets all of the Cliques.
    [Route("GetAllCliquesName")]
    [AllowAnonymous]
    [HttpGet]
    public async Task<List<String>> GetAllCliquesName()
    {

      List<Clique> theCliques =  await _cliqueService.GetAsync();
      List<String> cliqueNames = new List<String>();
      foreach(Clique clique in theCliques)
      {
        if(clique.Name != null)
        cliqueNames.Add(clique.Name);
      }


      return cliqueNames;
    }

    // This method gets a clique by its name
    [Route("{cliqueName}/GetCliqueByName")]
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetCliqueByName(String cliqueName)
    {

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.Listings == null)
      {
        return BadRequest("bad request");
      }
      else
      {
        Clique clique = new Clique();
        clique.Name = theClique.Name;
        clique.ProfilePic = theClique.ProfilePic;
        clique.Bio = theClique.Bio;
        return CreatedAtAction(nameof(GetCliqueByName), clique);
      }
    }

    // Gets all the posts, might need to scale this down later
    [HttpGet("{cliqueName}/GetPosts")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPosts(String cliqueName)
    {

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.Listings == null)
      {
        return BadRequest("bad request");
      }
      else
      {
        return CreatedAtAction(nameof(GetPosts), theClique.Postings);
      }
    }

    // Gets all the posts, might need to scale this down later, perhaps by adding a range parameter to it like (1-10)
    [HttpGet("{cliqueName}/GetListings")]
    [AllowAnonymous]
    public async Task<IActionResult> GetListings(String cliqueName)
    {

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.Listings == null)
      {
        return BadRequest("bad request");
      }
      else
      {
        return CreatedAtAction(nameof(GetListings), theClique.Listings);
      }
    }


    // User makes a post 
    [HttpPost("{cliqueName}/CreatePosting")]
    public async Task<IActionResult> CreatePosting(String cliqueName, Posting userPost)
    {
      if (await IsApproved((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if (!theClique.Postings.Any(b => b.postTitle == userPost.postTitle))
        {
          // Todo: ADD to the users posting as well.

          theClique.Postings.Add(userPost);
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          return NoContent();
        }
        else
        {
          return BadRequest("Post with same title exsist change the title to be unique");
        }
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }

    // User makes a listing 
    [HttpPost("{cliqueName}/CreateListing")]
    public async Task<IActionResult> CreateListing(String cliqueName, Listing userListing)
    {
      if (await IsApproved((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if (!theClique.Listings.Any(b => b.Title == userListing.Title))
        {
          userListing.Clique = cliqueName;
          await _listingService.CreateAsync(userListing); // Need to get an id for our user listing
          // image upload.
          string urlString = "";
          try
          {
            GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
            {
              BucketName = "manilovefrogs",
              Key = cliqueName+userListing.Id,
              Verb = HttpVerb.PUT,
              Expires = DateTime.UtcNow.AddMinutes(15),// Give 15 minutes for the preson to upload
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
          userListing.ImageKey = cliqueName + userListing.Id; // Set our image key for easy deletion
          userListing.ImageLink = "https://manilovefrogs.s3.us-west-1.amazonaws.com/" + cliqueName + userListing.Id;
          User theUser = await _userService.GetByUsername(getUserNameFromJWT((ClaimsIdentity)User.Identity));
          theUser.UserListings.Add(userListing);
          theClique.Listings.Add(userListing);
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          await _userService.UpdateAsync(theUser.Id, theUser);
          await _listingService.UpdateAsync(userListing.Id, userListing);
          return Ok(urlString);
        }
        else
        {
          return BadRequest("Listing with same title exsist change the title to be unique");
        }
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }
    /*
     *  Make a comment on a post
     */
    [HttpPut("{cliqueName}/{postTitle}/AddComment")]
    public async Task<IActionResult> AddComment(String cliqueName, String postTitle, [FromBody] String comment)
    {
      if (await IsApproved((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if (theClique.Postings.Any(b => b.postTitle == postTitle))
        {
          String username = getUserNameFromJWT((ClaimsIdentity)User.Identity);
          theClique.Postings.DistinctBy(b => b.postTitle == postTitle).FirstOrDefault().comments.Add(username + "\n" + comment); // Here we add the users comment to the post.
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          return NoContent();
        }
        else
        {
          return BadRequest("Post does not exist");
        }
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }



    private async Task<bool> IsApproved(ClaimsIdentity? theIdentity, String cliqueName)
    {
      if (theIdentity == null) return false;
      String username = getUserNameFromJWT(theIdentity);
      if (username == null) return false;
      // Now we reach into the list of users in the clique
      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null) return false;
      if (theClique.ApprovedUsers != null && theClique.ApprovedUsers.Contains(username))
      {
        return true;
      }
      return false;
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
