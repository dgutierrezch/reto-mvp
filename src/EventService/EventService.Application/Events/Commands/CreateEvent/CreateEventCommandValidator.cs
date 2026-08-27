using FluentValidation;

namespace EventService.Application.Events.Commands.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Date).NotEqual(default(DateTime));
        RuleFor(x => x.Zones).NotEmpty().WithMessage("Debe incluir al menos una zona.");

        RuleForEach(x => x.Zones).ChildRules(zone =>
        {
            zone.RuleFor(z => z.Name).NotEmpty().MaximumLength(100);
            zone.RuleFor(z => z.Price).GreaterThanOrEqualTo(0);
            zone.RuleFor(z => z.Capacity).GreaterThan(0);
        });
    }
}
