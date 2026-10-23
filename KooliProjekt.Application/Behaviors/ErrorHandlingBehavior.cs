using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KooliProjekt.Application.Behaviors
{
    public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TResponse : OperationResult, new()
    {
        private readonly ILogger<ErrorHandlingBehavior<TRequest, TResponse>> _logger;

        public ErrorHandlingBehavior(ILogger<ErrorHandlingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                _logger.LogError(ex, "Päringu töötlemine ebaõnnestus.");

                var response = new TResponse();

                response.AddError("Päringu töötlemine ebaõnnestus.");

                return response;
            }
        }
    }
}
