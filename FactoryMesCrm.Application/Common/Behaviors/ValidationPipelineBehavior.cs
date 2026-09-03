using FluentValidation;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace FactoryMesCrm.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline'ına giren tüm Command/Query isteklerini otomatik olarak FluentValidation ile doğrular.
/// Hata varsa isteği Handler'a ulaştırmadan fırlatır (Early Return/Fail-Fast).
/// </summary>
public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationFailures = _validators
            .Select(validator => validator.Validate(context))
            .SelectMany(validationResult => validationResult.Errors)
            .Where(validationFailure => validationFailure != null)
            .ToList();

        if (validationFailures.Any())
        {
            throw new ValidationException(validationFailures);
        }

        return await next();
    }
}