using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace OnlineStoreMVP.TestUtilities.Helpers;

/// <summary>
/// Provides helpers for creating FluentValidation mocks in tests.
/// </summary>
public static class ValidationHelpers
{
    /// <summary>
    /// Creates a mocked IValidator that always returns valid.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <returns>A mocked IValidator that returns valid results.</returns>
    public static Mock<IValidator<T>> CreateValidValidator<T>()
    {
        var mockValidator = new Mock<IValidator<T>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<T>()))
            .Returns(new ValidationResult());
        return mockValidator;
    }

    /// <summary>
    /// Creates a mocked IValidator with custom validation errors.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="errors">Dictionary of property names and their error messages.</param>
    /// <returns>A mocked IValidator that returns the specified validation errors.</returns>
    public static Mock<IValidator<T>> CreateInvalidValidator<T>(Dictionary<string, string[]> errors)
    {
        var mockValidator = new Mock<IValidator<T>>();
        var failures = errors.SelectMany(e =>
            e.Value.Select(v => new ValidationFailure(e.Key, v))).ToList();

        mockValidator.Setup(v => v.Validate(It.IsAny<T>()))
            .Returns(new ValidationResult(failures));

        return mockValidator;
    }

    /// <summary>
    /// Creates a mocked IValidator with a single validation error.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="propertyName">The property name with the error.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A mocked IValidator that returns the specified validation error.</returns>
    public static Mock<IValidator<T>> CreateInvalidValidator<T>(string propertyName, string errorMessage) 
        => CreateInvalidValidator<T>(new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        });
}