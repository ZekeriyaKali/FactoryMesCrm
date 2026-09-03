using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FactoryMesCrm.Application.Common.Behaviors;

/// <summary>
/// İsteklerin işlenme süresini ölçer. 500 ms üzerindeki yavaş sorguları ve komutları tespitedip uyarır.
/// </summary>
public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer;

    public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Processing Request: {Name}", requestName);

        _timer.Start();
        var response = await next();
        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        // 500 milisaniyenin üzerindeki işlemler yavaş işlem olarak loglanır (Performance Monitoring)
        if (elapsedMilliseconds > 500)
        {
            _logger.LogWarning("Long Running Request Detected: {Name} ({ElapsedMilliseconds} milliseconds)",
                requestName, elapsedMilliseconds);
        }

        _logger.LogInformation("Completed Request: {Name}", requestName);

        return response;
    }
}