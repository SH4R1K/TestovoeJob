namespace TestovoeJob.API.Models;

public sealed class ProcessingException : Exception
{
    public ProcessingException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public ProcessingException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
