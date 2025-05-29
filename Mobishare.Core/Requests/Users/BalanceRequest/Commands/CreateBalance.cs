using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.UserRelated;

namespace Mobishare.Core.Requests.Users.BalanceRequest.Commands;

public class CreateBalance : Balance, IRequest<Balance> { }

public class CreateBalanceHandler : IRequestHandler<CreateBalance, Balance>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateBalanceHandler> _logger;

    public CreateBalanceHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<CreateBalanceHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Balance> Handle(CreateBalance request, CancellationToken cancellationToken)
    {
        var newBalance = _mapper.Map<Balance>(request);
        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateBalance));
            _dbContext.Balances.Entry(newBalance).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Balance {balance} created successfully", newBalance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating balance");
        }

        return newBalance;
    }
}