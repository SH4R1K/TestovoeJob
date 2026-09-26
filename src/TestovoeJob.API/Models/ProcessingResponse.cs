namespace TestovoeJob.API.Models;

/// <summary>
/// Тело ответа POST /api/process. Ровно эти поля, сериализуются в snake_case.
/// Один и тот же DTO используется и для успеха, и для ошибок.
/// </summary>
public sealed class ProcessingResponse
{
    public int IsError { get; set; }

    public string ErrorCode { get; set; } = ProcessingErrorCodes.Ok;

    public string ErrorMessage { get; set; } = string.Empty;

    public int ElementsCount { get; set; }

    public int EmailsCount { get; set; }

    public string Url { get; set; } = string.Empty;

    public string DecryptedPlainText { get; set; } = string.Empty;

    public List<string> ElementsAttrList { get; set; } = [];

    public List<string> EmailsList { get; set; } = [];

    public static ProcessingResponse Success(
        string url,
        string decryptedPlainText,
        IReadOnlyList<ElementRecord> elements,
        IReadOnlyList<string> emails)
    {
        return new ProcessingResponse
        {
            IsError = 0,
            ErrorCode = ProcessingErrorCodes.Ok,
            ErrorMessage = string.Empty,
            ElementsCount = elements.Count,
            EmailsCount = emails.Count,
            Url = url,
            DecryptedPlainText = decryptedPlainText,
            ElementsAttrList = [.. elements.Select(static element => element.AttributeValue)],
            EmailsList = [.. emails]
        };
    }
    public static ProcessingResponse Failure(string errorCode, string errorMessage)
    {
        return new ProcessingResponse
        {
            IsError = 1,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            ElementsCount = 0,
            EmailsCount = 0,
            Url = string.Empty,
            DecryptedPlainText = string.Empty,
            ElementsAttrList = [],
            EmailsList = []
        };
    }
}
