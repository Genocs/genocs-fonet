using FluentValidation;

namespace Genocs.Fonet.WebApi.Models;

public sealed class PrintPdfRequestValidator : AbstractValidator<PrintPdfRequest>
{
    public PrintPdfRequestValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("TemplateId is required.");
        RuleFor(x => x.Model)
            .NotNull().WithMessage("Model is required.");
    }
}

