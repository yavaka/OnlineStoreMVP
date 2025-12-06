namespace OnlineStoreMVP.ServiceDefaults.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a request is invalid or cannot be processed due to client-side errors.
/// </summary>
/// <remarks>Use this exception to indicate that the client has provided invalid input or parameters that prevent
/// the request from being fulfilled. This exception is typically used in scenarios where the caller can correct the
/// request and try again.</remarks>
/// <param name="message">The error message that describes the reason for the bad request.</param>
public class BadRequestException(string message) : Exception(message)
{
}
