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
  [Route("Fish")]
  public class FishController : Controller
  {

    private readonly UserService _userService;
    private readonly CliqueService _cliqueService;
    private readonly IAmazonS3 _s3Client;
    private readonly FishService _fishService;
    public FishController(UserService userService, CliqueService cliqueService, IAmazonS3 s3Client, FishService fishService)
    {
      _userService = userService;
      _cliqueService = cliqueService;
      _s3Client = s3Client;
      _fishService = fishService;
    }
    // This method gets a list of all the Fish in the database.
    [Route("[action]")]
    [HttpGet]
    public async Task<List<Fish>> Get()
    {
      return await _fishService.GetAsync();
    }

    // This method gets a list of all the Fish Names in the database.
    [Route("GetAllFishByName")]
    [HttpGet]
    [AllowAnonymous]
    public async Task<List<String>> GetAllFishByName()
    {
     List<Fish> fishes = await _fishService.GetAsync();
     List<String> fishNames = new List<String>();
      foreach (var fish in fishes)
      {
        if (fish.Name != null)
        {
          fishNames.Add(fish.Name);
        }
      }
      fishNames.Sort(); // Hopefully this works with the defualt comparator watch.
      return fishNames;
    }

    // Retrieves a fish and its info by name
    [AllowAnonymous]
    [HttpGet("go/{name}")]
    public async Task<IActionResult> getFish(string name)
    {
      var theFish = await _fishService.GetByName(name);

      if (theFish is null)
      {
        return NotFound();
      }
      return CreatedAtAction(nameof(getFish), theFish);
    }

    // Retrieves a fish image
    [AllowAnonymous]
    [HttpGet("goImage/{name}")]
    public async Task<IActionResult> getFishImage(string name)
    {
      var theFish = await _fishService.GetByName(name);

      if (theFish is null)
      {
        return NotFound();
      }
      return CreatedAtAction(nameof(getFishImage), theFish.Pictures);
    }

    [Route("[action]")]
    [HttpPost]
    // Create a new Fish
    public async Task<IActionResult> Post(Fish fish)
    {
      if (fish == null || fish.Name == null)
      {
        return BadRequest("Null Clique given or Clique with null username");
      }
      // We can not allow duplicate usernames
      var dummyFish = await _fishService.GetByName(fish.Name);
      if (dummyFish != null)
      {
        return BadRequest("Fish with this name already exsists");

      }
      await _fishService.CreateAsync(fish);
      return CreatedAtAction(nameof(Get), new { id = fish.Id }, fish);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(string name)
    {
      var theFish = await _fishService.GetByName(name);

      if (theFish is null)
      {
        return NotFound();
      }
      await _fishService.RemoveByNameAsync(name); // Remove by the fish name as it is hard to get the ID in a ui context

      return NoContent();
    }



    /*
     * updates the Fish
     */
    [HttpPut("{name}")]
    public async Task<IActionResult> Update(string name, Fish updatedFish)
    {
      var fish = await _fishService.GetByName(name);

      if (fish is null)
      {
        return NotFound();
      }

      updatedFish.Id = fish.Id;

      await _fishService.UpdateAsync(fish.Id, updatedFish);

      return NoContent();
    }

  }


}
