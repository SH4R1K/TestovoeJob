namespace TestovoeJob.API.Models;

/// <summary>
/// Единый словарь кодов ошибок контракта API.
/// Строковые значения совпадают со значением error_code в ответе.
/// </summary>
public static class ProcessingErrorCodes
{
    public const string Ok = "OK";

    public const string InvalidJson = "INVALID_JSON";

    public const string ValidationError = "VALIDATION_ERROR";

    public const string InvalidBase64Url = "INVALID_BASE64_URL";

    public const string InvalidBase64Page = "INVALID_BASE64_PAGE";

    public const string InvalidBase64Key = "INVALID_BASE64_KEY";

    public const string InvalidBase64Ciphertext = "INVALID_BASE64_CIPHERTEXT";

    public const string InvalidKeyLength = "INVALID_KEY_LENGTH";

    public const string InvalidCiphertextLength = "INVALID_CIPHERTEXT_LENGTH";

    public const string InvalidSelector = "INVALID_SELECTOR";

    public const string InvalidUtf8 = "INVALID_UTF8";

    public const string DecryptionError = "DECRYPTION_ERROR";

    public const string DatabaseError = "DATABASE_ERROR";

    public const string InternalError = "INTERNAL_ERROR";
}
