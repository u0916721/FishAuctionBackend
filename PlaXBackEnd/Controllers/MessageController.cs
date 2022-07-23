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
  [Authorize]
  [Route("Messages")]
  public class MessageController : Controller
  {

    private readonly UserService _userService;
    private readonly CliqueService _cliqueService;
    private readonly IAmazonS3 _s3Client;
    private readonly MessageService _messageService;
    public MessageController(UserService userService, CliqueService cliqueService, IAmazonS3 s3Client, MessageService messageService)
    {
      _userService = userService;
      _cliqueService = cliqueService;
      _s3Client = s3Client;
      _messageService = messageService;
    }


    [HttpGet("get/conversations")]
    public async Task<IList<String>> GetConversations()
    {
      String username = getUserNameFromJWT((ClaimsIdentity)User.Identity);
      var user = await _userService.GetByUsername(username);

      if (user is null)
      {
        return null;
      }
      user.Password = null;
      user.Badges = null;
      return user.Messages;
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("deleteAll")]
    public async Task<IActionResult> deleteAll()
    {
      List<Message> m = await _messageService.GetAsync();
      foreach(Message message in m)
      {
        _messageService.RemoveAsync(message.Id);
      }
      List<User> u = await _userService.GetAsync();
      foreach(User user in u)
      {
        user.Messages = new List<String>();
        _userService.UpdateAsync(user.Id, user);
      }
      return Ok("Deleted all messages");
    }
    [HttpGet("get/conversation")]
    public async Task<IList<String>> GetConversation([FromBody] String messageTag)
    {
      String username = getUserNameFromJWT((ClaimsIdentity)User.Identity);
      // Message tag is of the format "name1" + "+" + "name2"
      // 

      var user = await _userService.GetByUsername(username);

      if (user is null)
      {
        return null;
      }

      String[] strings = messageTag.Split('+');
      if(strings[0].Equals(username) || strings[1].Equals(username)) // This is our verification that the user can see the conversation
      {
        Message theMessage = await _messageService.GetByTagName(messageTag);
        if(theMessage is null)
        {
          return null;
        }
        return theMessage.Messages;

      }
      
      return null;
    }


    [HttpPost("{userToSendTo}/SendMessage")]
    public async Task<IActionResult> SendMessage(String userToSendTo, [FromBody] String messageToSend)
    {
      String username = getUserNameFromJWT((ClaimsIdentity)User.Identity);
      // Message tag is of the format "name1" + "+" + "name2"
      // 

      User user = await _userService.GetByUsername(username);
      User userToSend = await _userService.GetByUsername(userToSendTo);
      if (user == null || userToSend == null)
      {
        return BadRequest("no valid users could be found to send to");
      }
      String[] potentialMessageTag = new String[2];
      potentialMessageTag[0] = userToSendTo;
      potentialMessageTag[1] = username;
      Array.Sort(potentialMessageTag);

      String messageTag = potentialMessageTag[0] + "+" + potentialMessageTag[1];

      Message theMessage = await _messageService.GetByTagName(messageTag);
      if(theMessage is null)
      {
        // then we need to send the message and store it in the db
        Message m = new Message();
        m.Messages = new List<String>();
        m.Messages.Add(username + "\n" + messageToSend); // Add the message
        m.Tag = messageTag; // Insert the tag we created earlier

        // Next we store it into the database
        _messageService.CreateAsync(m); // Create the new tag
        // Store that for the user to know whomst the are messgeing.
        if(user.Messages == null)
        {
          user.Messages = new List<String>();
        }
        if(userToSend.Messages == null)
        {
          userToSend.Messages = new List<String>();
        }
        user.Messages.Add(messageTag);
        userToSend.Messages.Add(messageTag);
        _userService.UpdateAsync(user.Id, user);
        _userService.UpdateAsync(userToSend.Id, userToSend);
        return Ok("New message object created");
      }
      else // We have a message already. 
      {
        theMessage.Messages.Add(username + "\n" + messageToSend); // Add the message.
        _messageService.UpdateAsync(theMessage.Id, theMessage);
        return Ok("Message added to existing conversation");
      }

    }
 




    //Helpers
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
