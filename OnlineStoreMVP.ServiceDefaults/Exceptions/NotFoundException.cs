namespace OnlineStoreMVP.ServiceDefaults.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a requested entity or resource cannot be found.
/// </summary>
/// <remarks>Use this exception to indicate that an operation failed because the specified entity does not exist.
/// This is commonly thrown in scenarios such as data retrieval or lookup operations when the target item is missing.
/// NotFoundException is typically used to provide more specific error information than a general Exception.</remarks>
public class NotFoundException : Exception
{
    /// <summary>
    /// Creates a new NotFoundException with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundException(string message) : base(message) { }

    /// <summary>
    /// Creates a new NotFoundException for the specified entity name and key.
    /// </summary>
    /// <param name="name">The name of the entity.</param>
    /// <param name="key">The key of the entity.</param>
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.") { }
}