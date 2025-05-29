using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.UserRelated;

namespace Mobishare.Core.Requests.Users.HistoryCreditRequest.Commands;

public class CreateHistoryCredit : HistoryCredit, IRequest<HistoryCredit> { }

public class CreateHistoryCreditHandler : IRequestHandler<CreateHistoryCredit, HistoryCredit>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateHistoryCreditHandler> _logger;

    public CreateHistoryCreditHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<CreateHistoryCreditHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HistoryCredit> Handle(CreateHistoryCredit request, CancellationToken cancellationToken)
    {
        var newHistoryCredit = _mapper.Map<HistoryCredit>(request);

        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateHistoryCredit));
            _dbContext.HistoryCredits.Entry(newHistoryCredit).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("History Credit {historyCredit} created successfully", newHistoryCredit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating history credit");
        }

        return newHistoryCredit;
    }
}
