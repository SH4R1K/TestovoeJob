using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TestovoeJob.API.Models;

namespace TestovoeJob.API.Services;

public sealed class ProcessingService
{
    private const int RequiredKeySizeBytes = 32;
    private const int AesBlockSizeBytes = 16;
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private static readonly Regex EmailRegex = new(
        @"(?<![\w.+-])[\w.+-]+@\w[\w-]*(?:\.[\w-]+)+(?![\w-])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(2));

    private readonly ElementRepository _repository;
    private readonly ILogger<ProcessingService> _logger;

    public ProcessingService(ElementRepository repository, ILogger<ProcessingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task<ProcessingResponse> ProcessAsync(
        ProcessingRequest request,
        CancellationToken cancellationToken = default)
    {
        string url = DecodeUtf8Strict(
            DecodeBase64(request.UrlB64, ProcessingErrorCodes.InvalidBase64Url, "url_b64"),
            ProcessingErrorCodes.InvalidUtf8,
            "url_b64");

        string page = DecodeUtf8Strict(
            DecodeBase64(request.PageB64, ProcessingErrorCodes.InvalidBase64Page, "page_b64"),
            ProcessingErrorCodes.InvalidUtf8,
            "page_b64");

        byte[] key = DecodeBase64(request.KeyBytesB64, ProcessingErrorCodes.InvalidBase64Key, "key_bytes_b64");
        if (key.Length != RequiredKeySizeBytes)
        {
            throw new ProcessingException(
                ProcessingErrorCodes.InvalidKeyLength,
                $"key_bytes_b64 должен декодироваться ровно в {RequiredKeySizeBytes} байт, получено {key.Length}.");
        }

        byte[] cipherText = DecodeBase64(
            request.EncryptedTextBytesB64,
            ProcessingErrorCodes.InvalidBase64Ciphertext,
            "encrypted_text_bytes_b64");
        if (cipherText.Length == 0 || cipherText.Length % AesBlockSizeBytes != 0)
        {
            throw new ProcessingException(
                ProcessingErrorCodes.InvalidCiphertextLength,
                $"encrypted_text_bytes_b64 должен быть непустым и кратным {AesBlockSizeBytes}, " +
                $"получено {cipherText.Length} байт.");
        }

        IReadOnlyList<ElementRecord> elements = ExtractElements(page, request.Selector, request.Attribute);

        IReadOnlyList<string> emails = ExtractEmails(page);

        string decryptedPlainText = Decrypt(key, cipherText);

        cancellationToken.ThrowIfCancellationRequested();
        await _repository.InsertAllAsync(elements, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Обработка завершена. Элементов: {ElementCount}. Email: {EmailsCount}.",
            elements.Count,
            emails.Count);

        return ProcessingResponse.Success(url, decryptedPlainText, elements, emails);
    }
    private static IReadOnlyList<ElementRecord> ExtractElements(string page, string selector, string attribute)
    {
        var parser = new HtmlParser();
        IDocument document = parser.ParseDocument(page);

        if (document.DocumentElement is null)
        {
            return [];
        }

        IHtmlCollection<IElement> matches;
        try
        {
            matches = document.QuerySelectorAll(selector);
        }
        catch (Exception exception) when (exception is DomException or ArgumentException)
        {
            throw new ProcessingException(
                ProcessingErrorCodes.InvalidSelector,
                $"selector не является валидным CSS selector: {exception.Message}",
                exception);
        }

        var elements = new List<ElementRecord>(matches.Length);
        foreach (IElement element in matches)
        {
            string attributeValue = element.GetAttribute(attribute) ?? string.Empty;
            elements.Add(new ElementRecord(attributeValue, element.OuterHtml));
        }

        return elements;
    }

    private static IReadOnlyList<string> ExtractEmails(string page)
    {
        MatchCollection matches = EmailRegex.Matches(page);
        var emails = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            emails.Add(match.Value);
        }

        return emails;
    }

    private static string Decrypt(byte[] key, byte[] cipherText)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            aes.Key = key;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

            return StrictUtf8.GetString(plainBytes);
        }
        catch (Exception exception) when (exception is CryptographicException or ArgumentException or DecoderFallbackException)
        {
            throw new ProcessingException(
                ProcessingErrorCodes.DecryptionError,
                $"Ошибка расшифровки: {exception.Message}",
                exception);
        }
    }

    private static byte[] DecodeBase64(string value, string errorCode, string fieldName)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException exception)
        {
            throw new ProcessingException(
                errorCode,
                $"{fieldName} не является корректным Base64: {exception.Message}",
                exception);
        }
    }

    private static string DecodeUtf8Strict(byte[] bytes, string errorCode, string fieldName)
    {
        try
        {
            return StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException exception)
        {
            throw new ProcessingException(
                errorCode,
                $"{fieldName} не является валидным UTF-8: {exception.Message}",
                exception);
        }
    }
}
