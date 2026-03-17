using FluentValidation;
using MediatR;

namespace Application.Core
{
    /// <summary>
    /// MediatR pipeline behavior that runs FluentValidation validators before any handler.
    /// Returns a failure Result when validation fails — no handler is invoked.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count == 0)
                return await next();

            // Build a Result<T> failure from validation errors if TResponse is Result<T>
            var responseType = typeof(TResponse);
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
                var failureMethod = responseType.GetMethod("Failure");
                return (TResponse)failureMethod!.Invoke(null, new object[] { errorMessage })!;
            }

            throw new ValidationException(failures);
        }
    }
}
