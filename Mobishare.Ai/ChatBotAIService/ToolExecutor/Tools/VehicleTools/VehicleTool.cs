using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Services.UserContext;

namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools.VehicleTools;

public class VehicleTool : IVehicleTool
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleTool> _logger;
    private readonly IUserContextService _userContext;
    public string? UserId { get; set; }

    public VehicleTool(
        IHttpClientFactory httpClientFactory,
        ILogger<VehicleTool> logger,
        IUserContextService userContext)
    {
        _httpClient = httpClientFactory.CreateClient("CityApi");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }
    public VehicleTool() { }
    
    public Task<string> ReportVehicleIssueAsync(string description)
    {
        
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
