using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Services.UserContext;

namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools.VehicleTools;

public class VehicleTool : IVehicleTool
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediatr;
    private readonly ILogger<VehicleTool> _logger;
    private readonly IUserContextService _userContext;
    public string? UserId { get; set; }

    public VehicleTool(IMapper mapper, IMediator mediatr, ILogger<VehicleTool> logger, IUserContextService userContext)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _mediatr = mediatr ?? throw new ArgumentNullException(nameof(mediatr));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }
    public VehicleTool() { }
    
    public Task<string> ReportVehicleIssueAsync(string description)
    {
        // implementa con il db
        // _logger.LogInformation("Segnalazione veicolo ricevuta: {description}", description);
        return Task.FromResult($"Segnalazione ricevuta: {description}");
    }

    public Task<string> ReserveVehicleAsync()
    {
        UserId = UserContext.UserId;
        // _logger.LogInformation(UserId);
        // _logger.LogInformation("Reserve");
        return Task.FromResult("Veicolo riservato con successo.");
    }
}
