using Microsoft.AspNetCore.Mvc;
using PlaXBackEnd.Models;
using PlaXBackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amazon.S3.Model;
using Amazon.S3;

namespace PlaXBackEnd.Controllers
{
  /*
   * This for me the admin when creating new cliques.
   * 
   */
  [ApiController]
  [Authorize(Roles = "Club")]
  [Route("Mod/Cliques")]
  public class CliqueModController : Controller
  {

    private readonly UserService _userService;
    private readonly CliqueService _cliqueService;
    private readonly IAmazonS3 _s3Client;
    public CliqueModController(UserService userService, CliqueService cliqueService, IAmazonS3 s3Client)
    {
      _userService = userService;
      _cliqueService = cliqueService;
      _s3Client = s3Client;
    }
    //approve users-x, disapprove users-x, make a pinned post - x, set catagories-x, get catagories-x, set badges-x, give user badges-x, 
    // delete post - x
    // delete listing - x
    // get requested users -x
    // 

    // Aprove users and disapprove users
    // Catagories
    // Get Catagories might need to move somewhere else
    [HttpGet("{cliqueName}/GetApprovedUsers")]
    [AllowAnonymous]
    public async Task<IActionResult> GetApprovedUsers(String cliqueName)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        return CreatedAtAction(nameof(GetApprovedUsers), theClique.ApprovedUsers);
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }

    // Aprove users and disapprove users
    // Catagories
    // Get Catagories might need to move somewhere else
    [HttpGet("{cliqueName}/GetRequestedUsers")]
    public async Task<IActionResult> GetRequestedUsers(String cliqueName)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        return CreatedAtAction(nameof(GetRequestedUsers), theClique.RequestedUsers);
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }

    [HttpPut("{cliqueName}/ApproveUser")]
    public async Task<IActionResult> Approve(String cliqueName, [FromBody] String userToApprove)
    {
      if(await IsMod((ClaimsIdentity)User.Identity,cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if(theClique.ApprovedUsers.Contains(userToApprove))
        {
          return BadRequest("User already apprioved");
        }
        else
        {
          theClique.ApprovedUsers.Add(userToApprove);
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          return NoContent(); 
        }
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }

    [HttpPut("{cliqueName}/DisapproveUser")]
    public async Task<IActionResult> Disapprove(String cliqueName, [FromBody] String userToApprove)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if (theClique.ApprovedUsers.Contains(userToApprove))
        {
          theClique.ApprovedUsers.Remove(userToApprove);
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          return NoContent(); // Remove user
        }
        else
        {
          return BadRequest("User not approved in the first place");
        }
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }

    // Manage club badges 
    [HttpPut("{cliqueName}/AddBadge")]
    public async Task<IActionResult> AddBadge(String cliqueName, Badge badgeToAdd)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if (theClique.Badges.Any(b => b.BadgeName == badgeToAdd.BadgeName))
        {
          return BadRequest("Bagde already present");
        }
        else
        {
          theClique.Badges.Add(badgeToAdd);
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          return NoContent();
        }
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }

    // Manage club badges 
    [HttpDelete("{cliqueName}/RemoveBadge")]
    public async Task<IActionResult> RemoveBadge(String cliqueName, Badge badgeToRemove)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if (theClique.Badges.Any(b => b.BadgeName == badgeToRemove.BadgeName))
        {
          Badge temp = theClique.Badges.DistinctBy(b => b.BadgeName == badgeToRemove.BadgeName).FirstOrDefault();
          theClique.Badges.Remove(temp);
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          return NoContent();
        }
        else
        {
          return BadRequest("No Bagde Found That Can Be Removed");
        }
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }

    [HttpGet("{cliqueName}/GetBadges")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBadges(String cliqueName)
    {
      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.Badges == null)
      {
        return BadRequest("bad request");
      }
      return CreatedAtAction(nameof(GetBadges), theClique.Badges);
      
    }

    // Catagories
    // Get Catagories might need to move somewhere else
    [HttpGet("{cliqueName}/GetCatagoires")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCatagoires(String cliqueName)
    {

        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.Catagories == null)
      {
        return BadRequest("bad request");
      }
      else
      {
        return CreatedAtAction(nameof(GetCatagoires), theClique.Catagories);
      }
      }


    // Update Catagories 
    [HttpPut("{cliqueName}/EditCatagories")]
    public async Task<IActionResult> EditCatagories(String cliqueName, Catagories updatedCatagories)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      
          theClique.Catagories = updatedCatagories;
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          return NoContent();       
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }

    // Get bio
    [HttpGet("{cliqueName}/GetBio")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBio(String cliqueName)
    {

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null)
      {
        if(theClique.Bio == null)
        {
          theClique.Bio = "Club has yet to set a bio";
        }
        return BadRequest("No Bio");
      }
      else
      {
        return CreatedAtAction(nameof(GetBio), theClique.Bio);
      }
    }


    // Edit Bio
    [HttpPut("{cliqueName}/EditBio")]
    public async Task<IActionResult> EditBio(String cliqueName, [FromBody] String bio)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);

        theClique.Bio = bio;
        await _cliqueService.UpdateAsync(theClique.Id, theClique);
        return NoContent();
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }

    // Make a pinned post 
    [HttpPut("{cliqueName}/PinPost")]
    public async Task<IActionResult> PinPost(String cliqueName, Posting pinnedPost)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if (!theClique.PinnedPostings.Any(b => b.postTitle == pinnedPost.postTitle))
        {
          theClique.PinnedPostings.Add(pinnedPost);
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

    // Remove a pinned post 
    [HttpDelete("{cliqueName}/RemovePinnedPost")]
    public async Task<IActionResult> RemovePinnedPost(String cliqueName, Posting pinnedPost)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if (theClique.PinnedPostings.Any(b => b.postTitle == pinnedPost.postTitle))
        {
          Posting toRemove = theClique.PinnedPostings.DistinctBy(b => b.postTitle == pinnedPost.postTitle).FirstOrDefault();
          theClique.PinnedPostings.Remove(toRemove);
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          return NoContent();
        }
        else
        {
          return BadRequest("No Post Found That Can Be Deleted");
        }
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }

    // Catagories
    // Get pinned post
    [HttpGet("{cliqueName}/GetPinnedPost")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPinnedPost(String cliqueName)
    {

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.PinnedPostings == null)
      {
        return BadRequest("bad request");
      }
      else
      {
        return CreatedAtAction(nameof(GetPinnedPost), theClique.PinnedPostings);
      }
    }

    // Remove a post 
    [HttpDelete("{cliqueName}/DeletePost")]
    public async Task<IActionResult> DeletePost(String cliqueName, Posting post)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if (theClique.Postings.Any(b => b.postTitle == post.postTitle))
        {
          Posting toRemove = theClique.PinnedPostings.DistinctBy(b => b.postTitle == post.postTitle).FirstOrDefault();
          theClique.Postings.Remove(toRemove);
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          return NoContent();
        }
        else
        {
          return BadRequest("No Post Found That Can Be Deleted");
        }
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }


    // Remove a listing 
    [HttpDelete("{cliqueName}/DeleteListing")]
    public async Task<IActionResult> DeleteListing(String cliqueName, Listing listing)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        if (theClique.Listings.Any(b => b.Id == listing.Id))
        {
          Listing toRemove = theClique.Listings.DistinctBy(b => b.Id == listing.Id).FirstOrDefault();
          theClique.Listings.Remove(toRemove);
          await _cliqueService.UpdateAsync(theClique.Id, theClique);
          return NoContent();
        }
        else
        {
          return BadRequest("No Listing Found That Can Be Deleted");
        }
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }


    [HttpGet("{cliqueName}/profilepic")]
    public async Task<IActionResult> GetPresignedURLProfilePic(String cliqueName)
    {
      if (await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        string urlString = "";
        try
        {
          GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
          {
            BucketName = "manilovefrogs",
            Key = cliqueName,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(15),// Give 15 minutes for the preson to upload
          };
          urlString = _s3Client.GetPreSignedURL(request1);
        }
        catch (AmazonS3Exception e)
        {
          return BadRequest("AWS ERROR");
        }
        catch (Exception e)
        {
          return BadRequest("AWS ERROR");
        }
        Clique theClique = await _cliqueService.GetByUsername(cliqueName);
        theClique.ProfilePic = "https://manilovefrogs.s3.us-west-1.amazonaws.com/" + cliqueName;
          await _cliqueService.UpdateAsync(theClique.Id, theClique);       
        return Ok(urlString);
      }
      return BadRequest("You do not have admin right to this auction, try logging in again");
    }



    // Helpers


    private async Task<bool> IsMod(ClaimsIdentity? theIdentity, String cliqueName)
    {
      if (theIdentity == null) return false;
      String username = getUserNameFromJWT(theIdentity);
      if (username == null) return false;
      // Now we reach into the list of users in the clique
      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null) return false;
      if (theClique.ApprovedUsers != null && theClique.Mods.Contains(username))
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
