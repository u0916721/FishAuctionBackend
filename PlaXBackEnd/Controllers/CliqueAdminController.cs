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
  [Authorize(Roles = "Admin")]
  [Route("Admin/Cliques")]
  public class CliqueAdminController : Controller
  {

    private readonly UserService _userService;
    private readonly CliqueService _cliqueService;
    private readonly IAmazonS3 _s3Client;
    public CliqueAdminController(UserService userService, CliqueService cliqueService, IAmazonS3 s3Client)
    {
      _userService = userService;
      _cliqueService = cliqueService;
      _s3Client = s3Client;
    }
    // This method gets a list of all the Cliques.
    [Route("[action]")]
    [HttpGet]
    public async Task<List<Clique>> Get()
    {
     return await _cliqueService.GetAsync();
    }



    [Route("[action]")]
    [HttpPost]
    // Create a new clique
    public async Task<IActionResult> Post(Clique clique)
    {
      if(clique == null || clique.Name == null)
      {
        return BadRequest("Null Clique given or Clique with null username");
      }
      // We can not allow duplicate usernames
      var dummyClique = await _cliqueService.GetByUsername(clique.Name);
      if(dummyClique != null)
      {
        return BadRequest("Clique with this name already exsists");

      }
      // Here we create objects so the fields not null on creation. to prevent null pointer exceptions
      if(clique.ApprovedUsers == null)
      clique.ApprovedUsers = new List<string>();
      if (clique.RequestedUsers == null)
        clique.RequestedUsers = new List<string>();
      if (clique.Badges == null)
        clique.Badges = new List<Badge>();
      if (clique.Catagories == null)
        clique.Catagories = new Catagories();
      if (clique.Bio == null)
        clique.Bio = "Create a bio with your club description";
      if (clique.Postings == null)
        clique.Postings = new List<Posting>();
      if (clique.PinnedPostings == null)
        clique.PinnedPostings = new List<Posting>();



      await _cliqueService.CreateAsync(clique);

      return CreatedAtAction(nameof(Get), new { id = clique.Id }, clique);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(string name)
    {
      var clique = await _cliqueService.GetByUsername(name);

      if (clique is null)
      {
        return NotFound();
      }
      var deleteRequest = new DeleteObjectRequest
      {

        Key = clique.Name,
        BucketName = "manilovefrogs",

      };
      // This deletes the cliques profile picture.
      await _s3Client.DeleteObjectAsync(deleteRequest);
      await _cliqueService.RemoveByUserNameAsync(name); // Remove by the username here as IDs are complicated. 

      return NoContent();
    }

    /*
     * updates the clique
     */
    [HttpPut("{name}")]
    public async Task<IActionResult> Update(string name, Clique updatedClique)
    {
      var clique = await _cliqueService.GetByUsername(name);

      if (clique is null)
      {
        return NotFound();
      }
      if (updatedClique.ApprovedUsers == null)
        updatedClique.ApprovedUsers = new List<string>();
      if (updatedClique.RequestedUsers == null)
        updatedClique.RequestedUsers = new List<string>();
      if (updatedClique.Badges == null)
        updatedClique.Badges = new List<Badge>();
      if (updatedClique.Catagories == null)
        updatedClique.Catagories = new Catagories();
      if (updatedClique.Bio == null)
        updatedClique.Bio = "Create a bio with your club description";
      if(updatedClique.Postings == null)
        updatedClique.Postings = new List<Posting>();
      if (updatedClique.PinnedPostings == null)
        updatedClique.PinnedPostings = new List<Posting>();

      updatedClique.Id = clique.Id;

      await _cliqueService.UpdateAsync(clique.Id, updatedClique);

      return NoContent();
    }

  }


}
