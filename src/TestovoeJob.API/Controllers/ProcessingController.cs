using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TestovoeJob.API.Models;
using TestovoeJob.API.Services;

namespace TestovoeJob.Api.Controllers;

[ApiController]
[Route("api/process")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class ProcessingController : ControllerBase
{
    private const long MaxRequestBodySizeBytes = 12L * 1024 * 1024;

    private readonly IValidator<ProcessingRequest> _validator;
    private readonly ProcessingService _processingService;
    private readonly ILogger<ProcessingController> _logger;

    public ProcessingController(
        IValidator<ProcessingRequest> validator,
        ProcessingService processingService,
        ILogger<ProcessingController> logger)
    {
        _validator = validator;
        _processingService = processingService;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(MaxRequestBodySizeBytes)]
    [ProducesResponseType(typeof(ProcessingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProcessingResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProcessingResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProcessingResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Process(
        [FromBody] ProcessingRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(ProcessingResponse.Failure(
                ProcessingErrorCodes.InvalidJson,
                "Тело запроса отсутствует или не является JSON-объектом."));
        }

        var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            string message = string.Join(
                " ",
                validationResult.Errors.Select(static error => error.ErrorMessage));

            return BadRequest(ProcessingResponse.Failure(ProcessingErrorCodes.ValidationError, message));
        }

        try
        {
            ProcessingResponse response = await _processingService
                .ProcessAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (ProcessingException exception)
        {
            _logger.LogWarning("Обработка не выполнена. error_code: {ErrorCode}. Причина: {Reason}",
                exception.ErrorCode, exception.Message);

            return StatusCode(MapErrorCodeToStatusCode(exception.ErrorCode), ProcessingResponse.Failure(
                exception.ErrorCode,
                exception.Message));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Непредвиденная ошибка при обработке запроса.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ProcessingResponse.Failure(ProcessingErrorCodes.InternalError, exception.Message));
        }
    }

    private static int MapErrorCodeToStatusCode(string errorCode) => errorCode switch
    {
        ProcessingErrorCodes.DatabaseError => StatusCodes.Status503ServiceUnavailable,
        ProcessingErrorCodes.InternalError => StatusCodes.Status500InternalServerError,
        ProcessingErrorCodes.Ok => StatusCodes.Status200OK,
        _ => StatusCodes.Status400BadRequest
    };
}
