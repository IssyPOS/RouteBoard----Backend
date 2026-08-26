using MediatR;
using Microsoft.Extensions.Logging;
using POSShopTicketing.Application.Common.Interfaces;

namespace POSShopTicketing.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehaviour(
        ILogger<LoggingBehaviour<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var teamMemberId = _currentUserService.TeamMemberId?.ToString() ?? "anonymous";

        _logger.LogInformation("POSShopTicketing request {RequestName} started by {TeamMemberId}", requestName, teamMemberId);

        return await next();
    }
}
