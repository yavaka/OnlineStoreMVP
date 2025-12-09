using Microsoft.Extensions.Logging;
using Moq;

namespace OnlineStoreMVP.TestUtilities.Helpers;

/// <summary>
/// Provides common mock creation methods for testing.
/// </summary>
public static class MockHelpers
{
    /// <summary>
    /// Creates a mocked ILogger instance for the specified type.
    /// </summary>
    /// <typeparam name="T">The type for which to create the logger.</typeparam>
    /// <returns>A mocked ILogger instance.</returns>
    public static Mock<ILogger<T>> CreateMockLogger<T>() => new Mock<ILogger<T>>();

    /// <summary>
    /// Creates a mocked ILogger instance with verification setup.
    /// </summary>
    /// <typeparam name="T">The type for which to create the logger.</typeparam>
    /// <param name="logLevel">The expected log level.</param>
    /// <param name="times">The number of times the log should be called.</param>
    /// <returns>A mocked ILogger instance configured for verification.</returns>
    public static Mock<ILogger<T>> CreateMockLoggerWithVerification<T>(
        LogLevel logLevel = LogLevel.Error,
        Times? times = null)
    {
        var mockLogger = new Mock<ILogger<T>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)))
            .Verifiable();

        return mockLogger;
    }
}
