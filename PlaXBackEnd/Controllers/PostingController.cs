using PlaXBackEnd.Models;
using PlaXBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PlaXBackEnd.Controllers;

[ApiController]
[Route("api/Posting")]
public class PostingController : ControllerBase
{
  private readonly ListingService _postingService;

  public PostingController(ListingService postingService) =>
      _postingService = postingService;

  [HttpGet]
  public async Task<List<Listing>> Get() =>
      await _postingService.GetAsync();

  [HttpGet("{id:length(24)}")]
  public async Task<ActionResult<Listing>> Get(string id)
  {
    var posting = await _postingService.GetAsync(id);

    if (posting is null)
    {
      return NotFound();
    }

    return posting;
  }

  [HttpPost]
  [Authorize]
  public async Task<IActionResult> Post(Listing newPosting)
  {

    var identity = (ClaimsIdentity)User.Identity;
    // Non null check here
    if(identity == null)
      return NotFound();

    IEnumerable<Claim> claims = identity.Claims;
    var test = claims?.FirstOrDefault(x => x.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", StringComparison.OrdinalIgnoreCase))?.Value; // Pull out users name.

    await _postingService.CreateAsync(newPosting);

    return CreatedAtAction(nameof(Get), new { id = newPosting.Id }, newPosting);
  }

  [HttpPut("{id:length(24)}")]
  public async Task<IActionResult> Update(string id, Listing updatedPosting)
  {
    var posting = await _postingService.GetAsync(id);

    if (posting is null)
    {
      return NotFound();
    }

    updatedPosting.Id = posting.Id;

    await _postingService.UpdateAsync(id, updatedPosting);

    return NoContent();
  }

  [HttpDelete("{id:length(24)}")]
  public async Task<IActionResult> Delete(string id)
  {
    var posting = await _postingService.GetAsync(id);

    if (posting is null)
    {
      return NotFound();
    }

    await _postingService.RemoveAsync(id);

    return NoContent();
  }
}