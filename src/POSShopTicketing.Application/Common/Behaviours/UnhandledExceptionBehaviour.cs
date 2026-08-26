using MediatR;
using Microsoft.Extensions.Logging;
using POSShopTicketing.Application.Common.Exceptions;

namespace POSShopTicketing.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehaviour(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex) when (ex is not ValidationException
                                     and not NotFoundException
                                     and not UnauthorizedException
                                     and not ForbiddenException
                                     and not POSShopTicketing.Domain.Exceptions.DomainException)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogError(ex, "POSShopTicketing request {RequestName} failed unexpectedly", requestName);
            throw;
        }
    }
}
