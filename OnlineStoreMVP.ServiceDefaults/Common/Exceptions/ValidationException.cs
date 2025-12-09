namespace OnlineStoreMVP.ServiceDefaults.Common.Exceptions;

/// <summary>
/// Represents an exception that is thrown when one or more validation failures occur.
/// </summary>
/// <param name="errors">A dictionary containing validation errors, where each key is the name of a field and the value is an array of error
/// messages associated with that field.</param>
public class ValidationException(Dictionary<string, string[]> errors) : Exception("One or more validation failures have occurred.")
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; } = errors;
}
