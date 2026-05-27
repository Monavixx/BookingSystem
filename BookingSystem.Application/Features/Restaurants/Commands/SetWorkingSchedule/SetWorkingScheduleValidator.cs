using FluentValidation;

namespace BookingSystem.Application.Features.Restaurants.Commands.SetWorkingSchedule;

public class SetWorkingScheduleValidator : AbstractValidator<SetWorkingScheduleCommand>
{
    public SetWorkingScheduleValidator()
    {
        RuleForEach(x=>x.Schedules)
            .ChildRules(v =>
            {
                v.When(x => x.IsClosed is not true, () =>
                {
                    v.RuleFor(x => x.OpeningTime)
                        .NotNull();
                    v.RuleFor(x => x.ClosingTime)
                        .NotNull();
                });
            });
        RuleFor(x => x.Schedules.Count())
            .Equal(7);
    }
}