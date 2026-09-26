namespace TestovoeJob.API.Models;

public sealed class ProcessingRequest
{
    public string UrlB64 { get; set; } = string.Empty;

    public string PageB64 { get; set; } = string.Empty;

    public string Selector { get; set; } = string.Empty;

    public string Attribute { get; set; } = string.Empty;

    public string EncryptedTextBytesB64 { get; set; } = string.Empty;

    public string KeyBytesB64 { get; set; } = string.Empty;
}

public static class ProcessingRequestLimits
{
    public const int UrlBase64MaxLength = 8 * 1024;

    public const int PageBase64MaxLength = 8 * 1024 * 1024;

    public const int SelectorMaxLength = 1024;

    public const int AttributeMaxLength = 256;

    public const int KeyBase64MaxLength = 1024;

    public const int CiphertextBase64MaxLength = 8 * 1024 * 1024;
}
