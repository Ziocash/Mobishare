using System;
using Microsoft.AspNetCore.Mvc;

namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools;

    [ApiController]
    [Route("api/UserContext")]
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
        try
        {
            if (location == null)
            {
                return BadRequest("Location data is null");
            }

            UserContext.Lat = location.Lat.ToString();
            UserContext.Lon = location.Lon.ToString();
            
            return Ok($"Location set: Lat={location.Lat}, Lon={location.Lon}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
    }

    public class LocationDto
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
