using System;

namespace Mobishare.Core.Services.UserContext;

public interface IUserContextService
{
    string? UserId { get; set; }
    string? Lat { get; set; }
    string? Lon { get; set; }
}
