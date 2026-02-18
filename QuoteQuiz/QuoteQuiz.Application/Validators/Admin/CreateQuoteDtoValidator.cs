using FluentValidation;
using QuoteQuiz.Application.DTOs.Admin;

namespace QuoteQuiz.Application.Validators.Admin;

public class CreateQuoteDtoValidator : AbstractValidator<CreateQuoteDto>
{
    public CreateQuoteDtoValidator()
    {
        RuleFor(x => x.QuoteText)
            .NotEmpty().WithMessage("Quote text is required")
            .MaximumLength(1000).WithMessage("Quote text must not exceed 1000 characters");

        RuleFor(x => x.AuthorName)
            .NotEmpty().WithMessage("Author name is required")
            .MaximumLength(100).WithMessage("Author name must not exceed 100 characters");
    }
}