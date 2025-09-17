
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Services.UserContext;
//using Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools;
namespace Mobishare.Core.Controllers.Users
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserContextController : ControllerBase
    {
        // private readonly IUserContextService _userContext;

        // public UserContextController(IUserContextService userContext)
        // {
        //     _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        // }

        [HttpPost("SetLocation")]
        public IActionResult SetLocation([FromBody] LocationDto location)
        {
           // UserContext.Lat = location.Lat.ToString();
           // UserContext.Lon = location.Lon.ToString();
            return Ok();
        }
    }

    public class LocationDto
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}