using FluentValidation;
using QuoteQuiz.Application.DTOs.Quiz;

namespace QuoteQuiz.Application.Validators.Quiz;

public class SubmitAnswerDtoValidator : AbstractValidator<SubmitAnswerDto>
{
    public SubmitAnswerDtoValidator()
    {
        RuleFor(x => x.QuoteId)
            .GreaterThan(0).WithMessage("Invalid quote");

        RuleFor(x => x.SelectedAnswer)
            .NotEmpty().WithMessage("Answer is required");

        RuleFor(x => x.QuizMode)
            .NotEmpty().WithMessage("Quiz mode is required")
            .Must(mode => mode == "Binary" || mode == "MultipleChoice")
            .WithMessage("Quiz mode must be 'Binary' or 'MultipleChoice'");
    }
}