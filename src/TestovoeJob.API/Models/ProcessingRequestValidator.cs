using FluentValidation;

namespace TestovoeJob.API.Models;

public sealed class ProcessingRequestValidator : AbstractValidator<ProcessingRequest>
{
    public ProcessingRequestValidator()
    {
        RuleFor(request => request.UrlB64)
            .NotEmpty().WithMessage("url_b64 обязателен и не должен быть пустым.")
            .MaximumLength(ProcessingRequestLimits.UrlBase64MaxLength)
                .WithMessage($"url_b64 не должен быть длиннее {ProcessingRequestLimits.UrlBase64MaxLength} символов.");

        RuleFor(request => request.PageB64)
            .NotEmpty().WithMessage("page_b64 обязателен и не должен быть пустым.")
            .MaximumLength(ProcessingRequestLimits.PageBase64MaxLength)
                .WithMessage($"page_b64 не должен быть длиннее {ProcessingRequestLimits.PageBase64MaxLength} символов.");

        RuleFor(request => request.Selector)
            .NotEmpty().WithMessage("selector обязателен и не должен быть пустым.")
            .MaximumLength(ProcessingRequestLimits.SelectorMaxLength)
                .WithMessage($"selector не должен быть длиннее {ProcessingRequestLimits.SelectorMaxLength} символов.");

        RuleFor(request => request.Attribute)
            .NotEmpty().WithMessage("attribute обязателен и не должен быть пустым.")
            .MaximumLength(ProcessingRequestLimits.AttributeMaxLength)
                .WithMessage($"attribute не должен быть длиннее {ProcessingRequestLimits.AttributeMaxLength} символов.");

        RuleFor(request => request.EncryptedTextBytesB64)
            .NotEmpty().WithMessage("encrypted_text_bytes_b64 обязателен и не должен быть пустым.")
            .MaximumLength(ProcessingRequestLimits.CiphertextBase64MaxLength)
                .WithMessage($"encrypted_text_bytes_b64 не должен быть длиннее {ProcessingRequestLimits.CiphertextBase64MaxLength} символов.");

        RuleFor(request => request.KeyBytesB64)
            .NotEmpty().WithMessage("key_bytes_b64 обязателен и не должен быть пустым.")
            .MaximumLength(ProcessingRequestLimits.KeyBase64MaxLength)
                .WithMessage($"key_bytes_b64 не должен быть длиннее {ProcessingRequestLimits.KeyBase64MaxLength} символов.");
    }
}
