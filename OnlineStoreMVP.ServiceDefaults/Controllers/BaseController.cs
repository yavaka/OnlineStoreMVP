using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnlineStoreMVP.ServiceDefaults.Exceptions;
using OnlineStoreMVP.ServiceDefaults.Models;
using System.Diagnostics;

namespace OnlineStoreMVP.ServiceDefaults.Controllers;

/// <summary>
/// Base controller that provides common exception handling functionality for derived controllers.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Generates an appropriate HTTP response for the specified exception, mapping known exception types to
    /// corresponding status codes and error details.
    /// </summary>
    /// <remarks>This method maps specific exception types, such as NotFoundException, BadRequestException,
    /// and ValidationException, to their respective HTTP status codes. All other exceptions are treated as internal
    /// server errors. The response includes a trace identifier for correlation and logging purposes.</remarks>
    /// <param name="ex">The exception to handle. Determines the type of response returned based on its type.</param>
    /// <param name="logger">The logger used to record details about the exception and the handling process.</param>
    /// <param name="operation">An optional operation name or identifier to include in the error response for additional context.</param>
    /// <returns>An <see cref="IActionResult"/> representing the HTTP response corresponding to the exception type. Returns a
    /// response with a suitable status code and error information.</returns>
    protected IActionResult HandleException(Exception ex, ILogger logger, string? operation = null)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        return ex switch
        {
            NotFoundException notFoundEx => HandleNotFound(notFoundEx, traceId, logger),
            BadRequestException badRequestEx => HandleBadRequest(badRequestEx, traceId, logger),
            ValidationException validationEx => HandleValidationError(validationEx, traceId, logger),
            _ => HandleInternalServerError(ex, traceId, logger, operation)
        };
    }

    /// <summary>
    /// Handles NotFoundException and returns a 404 Not Found response.
    /// </summary>
    /// <param name="ex">The exception to handle.</param>
    /// <param name="traceId">The trace identifier for the request.</param>
    /// <param name="logger">The logger used to record details about the exception and the handling process.</param>
    /// <returns>An <see cref="IActionResult"/> representing the HTTP response corresponding to the exception type.</returns>
    protected IActionResult HandleNotFound(NotFoundException ex, string traceId, ILogger logger)
    {
        logger.LogWarning(ex, "Not found error: {Message}", ex.Message);

        var errorResponse = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = ex.Message,
            Instance = HttpContext.Request.Path,
            TraceId = traceId
        };

        return NotFound(errorResponse);
    }

    /// <summary>
    /// Handles BadRequestException and returns a 400 Bad Request response.
    /// </summary>
    /// <param name="ex">The exception to handle.</param>
    /// <param name="traceId">The trace identifier for the request.</param>
    /// <param name="logger">The logger used to record details about the exception and the handling process.</param>
    /// <returns>An <see cref="IActionResult"/> representing the HTTP response corresponding to the exception type.</returns>
    protected IActionResult HandleBadRequest(BadRequestException ex, string traceId, ILogger logger)
    {
        logger.LogWarning(ex, "Bad request error: {Message}", ex.Message);

        var errorResponse = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = ex.Message,
            Instance = HttpContext.Request.Path,
            TraceId = traceId
        };

        return BadRequest(errorResponse);
    }

    /// <summary>
    /// Creates a Bad Request (400) response containing details about a validation error.
    /// </summary>
    /// <remarks>The response includes error details from the exception and sets the trace identifier for
    /// diagnostic purposes. The error format follows RFC 7231 section 6.5.1 for client error responses.</remarks>
    /// <param name="ex">The validation exception that contains error details to include in the response. Cannot be null.</param>
    /// <param name="traceId">The trace identifier associated with the request, used for correlation in logs and error responses.</param>
    /// <param name="logger">The logger used to record the validation error event.</param>
    /// <returns>An IActionResult representing a Bad Request response with a structured error payload describing the validation
    /// failure.</returns>
    protected IActionResult HandleValidationError(ValidationException ex, string traceId, ILogger logger)
    {
        logger.LogWarning(ex, "Validation error: {Message}", ex.Message);

        var errorResponse = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = ex.Message,
            Errors = ex.Errors,
            Instance = HttpContext.Request.Path,
            TraceId = traceId
        };

        return BadRequest(errorResponse);
    }

    /// <summary>
    /// Creates a standardized 500 Internal Server Error response containing error details and a trace identifier.
    /// </summary>
    /// <remarks>In development environments, the response includes detailed exception information. In
    /// production, only a generic error message is returned. The response conforms to RFC 7231 section 6.6.1 for error
    /// representation.</remarks>
    /// <param name="ex">The exception that triggered the internal server error response.</param>
    /// <param name="traceId">A unique identifier for tracing the request associated with the error.</param>
    /// <param name="logger">The logger used to record the error details.</param>
    /// <param name="operation">The name of the operation during which the exception occurred, or <see langword="null"/> if unspecified.</param>
    /// <returns>An <see cref="IActionResult"/> representing a 500 Internal Server Error response with error information.</returns>
    protected IActionResult HandleInternalServerError(Exception ex, string traceId, ILogger logger, string? operation)
    {
        logger.LogError(ex, "Unhandled exception during {Operation}: {Message}", operation ?? "operation", ex.Message);

        var errorResponse = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "An error occurred while processing your request",
            Status = StatusCodes.Status500InternalServerError,
            Detail = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                ? ex.ToString()
                : "An error occurred while processing your request.",
            Instance = HttpContext.Request.Path,
            TraceId = traceId
        };

        return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
    }
}