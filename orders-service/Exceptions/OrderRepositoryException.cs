/// <summary>
/// Custom exception for repository-level errors in the OrderRepository.
/// This exception should be thrown when a database or data access error occurs.
/// </summary>
public class OrderRepositoryException : Exception
{
    /// <summary>
    /// Constructor that takes only a message.
    /// This is useful when an error occurs, but there is no inner exception to track.
    /// </summary>
    /// <param name="message">The error message describing the issue.</param>
    public OrderRepositoryException(string message)
        : base(message) { }

    /// <summary>
    /// Constructor that takes both a message and an inner exception.
    /// This is useful when wrapping an underlying exception (e.g., a database error).
    /// It preserves the original exception details for debugging.
    /// </summary>
    /// <param name="message">The error message describing the issue.</param>
    /// <param name="innerException">The original exception that caused this error.</param>
    public OrderRepositoryException(string message, Exception innerException)
        : base(message, innerException) { }
}
