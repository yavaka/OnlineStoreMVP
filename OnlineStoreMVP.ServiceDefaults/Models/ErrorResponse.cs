namespace OnlineStoreMVP.ServiceDefaults.Models;

/// <summary>
/// Represents a standardized error response model.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// A URI reference that identifies the problem type.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the title associated with the object.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the status code representing the current state of the object.
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// Gets or sets the detailed description or additional information associated with the object.
    /// </summary>
    public string Detail { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the instance identifier associated with the object.
    /// </summary>
    public string? Instance { get; set; }
    /// <summary>
    /// Gets or sets the errors associated with the object.
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier used to trace the operation across system boundaries.
    /// </summary>
    public string? TraceId { get; set; }
}
