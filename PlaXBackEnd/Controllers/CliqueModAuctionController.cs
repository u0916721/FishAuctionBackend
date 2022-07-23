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
  [Authorize(Roles = "Admin,Club")]
  [Route("Cliques/Auction")]
  public class CliqueModAuction : Controller
  {

    private readonly UserService _userService;
    private readonly CliqueService _cliqueService;
    private readonly IAmazonS3 _s3Client;
    public CliqueModAuction(UserService userService, CliqueService cliqueService, IAmazonS3 s3Client)
    {
      _userService = userService;
      _cliqueService = cliqueService;
      _s3Client = s3Client;
    }
    // Gets all the upcoming items in the clique. 
    [HttpGet("{cliqueName}/GetItems")]
    [AllowAnonymous]
    public async Task<IActionResult> GetItems(String cliqueName)
    {

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null)
      {
        return BadRequest("Clique does not exist");
      }
      if (theClique.Items == null) // Catching said edge case.
      {
        theClique.Items = new List<Item>();
      }
      return CreatedAtAction(nameof(GetItems), theClique.Items);
    }


    // This method gets a list of all the sold items
    [HttpGet("{cliqueName}/GetSoldItems")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSoldItems(String cliqueName)
    {

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null)
      {
        return BadRequest("Clique does not exist");
      }
      if (theClique.ItemsSold == null) // Catching said edge case.
      {
        theClique.ItemsSold = new List<Item>();
      }
      return CreatedAtAction(nameof(GetSoldItems), theClique.ItemsSold);
    }

    [Route("{cliqueName}/AddItem")]
    [HttpPost]
    // Adds an item to the clique
    public async Task<IActionResult> AddItem(String cliqueName, [FromBody]Item item)
    {
      if (!await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        return BadRequest("You do not have admin right to this auction, try logging in again");
      }
      if (item == null || item.Seller == null)
      {
        return BadRequest("Null Item given or Clique with null username");
      }
      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      // Create a new list if one does not exsist
      if(theClique.Items == null)
      {
        theClique.Items = new List<Item>();
      }
      if (theClique.ItemsSold == null)
      {
        theClique.ItemsSold = new List<Item>();
      }
      Random rnd = new Random();

        if(item.ItemName == null)
        {
          item.ItemName = " ";
        }
        if (item.ImageLink == null)
        {
          item.ImageLink = " ";
        }

      item.ItemName = generateUniqueId(theClique, item);

      theClique.Items.Add(item);
      await _cliqueService.UpdateAsync(theClique.Id, theClique);
      return NoContent();
    }

    private String generateUniqueId(Clique theClique, Item item)
    {
      Random rnd = new Random();
      item.ItemName = item.ItemName.Trim();
      if (item.ItemName.Contains("%"))
      {
        item.ItemName = item.ItemName.Split("%")[0];
      }
      System.Text.StringBuilder builder = new System.Text.StringBuilder(item.ItemName);
      builder.Append("%");
      while (theClique.Items.Any(b => b.ItemName == builder.ToString()) || theClique.ItemsSold.Any(b => b.ItemName == builder.ToString())) // Make sure we generate a random item name
      {

        builder.Append(rnd.Next(1, 10) + "");
      }

      return builder.ToString();
    }

    [HttpPut("{cliqueName}/PopItem")]
    public async Task<IActionResult> PopItem(string cliqueName, int amount, string buyer)
    {
      if (!await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        return BadRequest("You do not have admin right to this auction, try logging in again");
      }

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if(theClique == null || theClique.Items == null || theClique.Items.Count < 1)
      {
        return BadRequest("No items to pop");
      }
      Item itemRemoved = theClique.Items[0];
      if(theClique.ItemsSold == null)
      {
        theClique.ItemsSold = new List<Item>();
      }
      theClique.Items.Remove(itemRemoved);
      itemRemoved.Buyer = buyer;
      itemRemoved.SoldFor = amount;
      theClique.ItemsSold.Insert(0,itemRemoved);
      await _cliqueService.UpdateAsync(theClique.Id, theClique);
      return NoContent();
    }
    // Remove from the sold list
    [HttpPut("{cliqueName}/DeleteItem")]
    public async Task<IActionResult> DeleteItem(string cliqueName, int itemIndex)
    {
      if (!await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        return BadRequest("You do not have admin right to this auction, try logging in again");
      }

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.Items == null || theClique.Items.Count < 1)
      {
        return BadRequest("No items to remove");
      }
      
      if(itemIndex > theClique.Items.Count - 1)
      {
        return BadRequest("Out of range");
      }
        Item itemRemoved = theClique.Items[itemIndex];
        theClique.Items.Remove(itemRemoved);
        await _cliqueService.UpdateAsync(theClique.Id, theClique);
     
      return NoContent();
    }
    // remove from the Item sold list.
    [HttpPut("{cliqueName}/DeleteItemFromSold")]
    public async Task<IActionResult> DeleteItemFromSold(string cliqueName, int itemIndex)
    {
      if (!await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        return BadRequest("You do not have admin right to this auction, try logging in again");
      }

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.ItemsSold == null || theClique.ItemsSold.Count < 1)
      {
        return BadRequest("No items to remove");
      }
      if(itemIndex > theClique.ItemsSold.Count - 1)
      {
        return BadRequest("Item out of range");
      }
      Item itemRemoved = theClique.ItemsSold[itemIndex];
      theClique.ItemsSold.Remove(itemRemoved);
      await _cliqueService.UpdateAsync(theClique.Id, theClique);
      return NoContent();
    }

    //Swaps an item placement
    [HttpPut("{cliqueName}/SwapItem")]
    public async Task<IActionResult> SwapItem(string cliqueName, [FromBody] Swap s)
    {
      if (!await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        return BadRequest("You do not have admin right to this auction, try logging in again");
      }

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.Items == null || theClique.Items.Count < 2)
      {
        return BadRequest("Cant swap");
      }
      try
      {
        // Swap occurs here.
        Item temp = theClique.Items[s.From];
        Item temp2 = theClique.Items[s.To];
        theClique.Items[s.From] = temp2;
        theClique.Items[s.To] = temp;
        await _cliqueService.UpdateAsync(theClique.Id, theClique);
        return NoContent();
      }
      catch(Exception e)
      {
        return BadRequest("Invalid indexs to swap" + e.Message);
      }
    }

    //Swaps an item placement
    [HttpPut("{cliqueName}/PutItemFront")]
    public async Task<IActionResult> PutItemFront(string cliqueName, [FromBody] int itemIndex)
    {
      if (!await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        return BadRequest("You do not have admin right to this auction, try logging in again");
      }

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.Items == null || theClique.Items.Count < 2)
      {
        return BadRequest("Cant push to front");
      }
      try
      {
        //O(N)
        // push here
        Item tempItem = theClique.Items[itemIndex];
        theClique.Items.RemoveAt(itemIndex);
        theClique.Items = theClique.Items.Prepend(tempItem).ToList();
        await _cliqueService.UpdateAsync(theClique.Id, theClique);
        return NoContent();
      }
      catch (Exception e)
      {
        return BadRequest("Invalid indexs to swap" + e.Message);
      }
    }
    [Route("{cliqueName}/{itemName}/UpdateItemSelling")]
    [HttpPut]
    // Adds an item to the clique
    public async Task<IActionResult> UpdateItemSelling(String cliqueName, String itemName, [FromBody] Item item)
    {
      if (!await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        return BadRequest("You do not have admin right to this auction, try logging in again");
      }

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.Items == null)
      {
        return BadRequest("No items to edit");
      }
      try
      {
        Item i = theClique.Items.Where(x => x.ItemName == itemName).FirstOrDefault();
        i.Seller = item.Seller;
        i.ImageLink = item.ImageLink;
        i.SoldFor = item.SoldFor;
        i.Fish = item.Fish;
        i.Buyer = item.Buyer;
        i.Description = item.Description;
        

        i.ItemName = generateUniqueId(theClique, item);
        // push here
        await _cliqueService.UpdateAsync(theClique.Id, theClique);
        return Ok(i.ItemName);
      }
      catch (Exception e)
      {
        return BadRequest("Invalid indexs to swap" + e.Message);
      }
    }


    [Route("{cliqueName}/{itemName}/UpdateItemSold")]
    [HttpPut]
    // Adds an item to the clique
    public async Task<IActionResult> UpdateItemSold(String cliqueName, String itemName, [FromBody] Item item)
    {
      if (!await IsMod((ClaimsIdentity)User.Identity, cliqueName))
      {
        return BadRequest("  var responseOk;");
      }

      Clique theClique = await _cliqueService.GetByUsername(cliqueName);
      if (theClique == null || theClique.ItemsSold == null)
      {
        return BadRequest("No items to edit");
      }
      try
      {
        Item i = theClique.ItemsSold.Where(x => x.ItemName == itemName).FirstOrDefault();
        i.Seller = item.Seller;
        i.ImageLink = item.ImageLink;
        i.SoldFor = item.SoldFor;
        i.Fish = item.Fish;
        i.Buyer = item.Buyer;
        i.Description = item.Description;

        i.ItemName = generateUniqueId(theClique, item);
        //O(N)
        // push here
        await _cliqueService.UpdateAsync(theClique.Id, theClique);
        return Ok(i.ItemName);
      }
      catch (Exception e)
      {
        return BadRequest("can not edit item");
      }
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
      if (updatedClique.Postings == null)
        updatedClique.Postings = new List<Posting>();
      if (updatedClique.PinnedPostings == null)
        updatedClique.PinnedPostings = new List<Posting>();

      updatedClique.Id = clique.Id;

      await _cliqueService.UpdateAsync(clique.Id, updatedClique);

      return NoContent();
    }


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
