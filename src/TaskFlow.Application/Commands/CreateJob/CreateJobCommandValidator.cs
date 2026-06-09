using FluentValidation;

namespace TaskFlow.Application.Commands.CreateJob;

public class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(x => x.JobType)
            .NotEmpty().WithMessage("JobType is required.")
            .MaximumLength(100).WithMessage("JobType must not exceed 100 characters.");

        RuleFor(x => x.Payload)
            .NotEmpty().WithMessage("Payload is required.")
            .MaximumLength(65536).WithMessage("Payload must not exceed 64KB.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("Idempotency-Key header is required.")
            .MaximumLength(200).WithMessage("Idempotency-Key must not exceed 200 characters.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority must be Low or High.");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ScheduledAt.HasValue)
            .WithMessage("ScheduledAt must be a future date.");
    }
}
