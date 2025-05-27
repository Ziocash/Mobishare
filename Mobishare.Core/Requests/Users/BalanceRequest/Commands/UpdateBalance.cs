using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.UserRelated;

namespace Mobishare.Core.Requests.Users.BalanceRequest.Commands;

public class UpdateBalance : Balance, IRequest<Balance>{}

public class UpdateBalanceHandler : IRequestHandler<UpdateBalance, Balance>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UpdateBalanceHandler> _logger;

    public UpdateBalanceHandler(ApplicationDbContext dbContext, ILogger<UpdateBalanceHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Balance> Handle(UpdateBalance request, CancellationToken cancellationToken)
    {
        var existingBalance = await _dbContext.Balances.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (existingBalance == null)
        {
            _logger.LogWarning("Balance with ID {Id} not found", request.Id);
            return null;
        }

        try
        {
            _logger.LogDebug("Executing {method}", nameof(UpdateBalanceHandler));

            if (existingBalance.Credit != request.Credit)
                existingBalance.Credit = request.Credit;
            if (existingBalance.Points != request.Points)
                existingBalance.Points = request.Points;
            if (existingBalance.UserId != request.UserId)
                existingBalance.UserId = request.UserId;

            _dbContext.Balances.Update(existingBalance);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Balance {balanceId} updated successfully", existingBalance.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance {balanceId}", request.Id);
            throw;
        }

        return existingBalance;
    }
}