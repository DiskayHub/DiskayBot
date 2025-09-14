using DiskayBot.API.Contracts.Schedule;
using FluentValidation;

namespace DiskayBot.API.Validators;

public class DayScheduleRequestValidator : AbstractValidator<DayScheduleRequest> {
    public DayScheduleRequestValidator() {
        RuleFor(d => d.d_start)
            .NotEmpty()
            .Matches(@"^\d{4}-\d{2}-\d{2}$")
            .WithMessage("Дата должна быть в формате ГГГГ-ММ-ДД");
        RuleFor(d => d.d_end)
            .NotEmpty()
            .Matches(@"^\d{4}-\d{2}-\d{2}$")
            .WithMessage("Дата должна быть в формате ГГГГ-ММ-ДД");
        RuleFor(d => d.group)
            .NotEmpty()
            .Matches(@"^ИТ\d{2}-\d{2}$")
            .WithMessage("Группа должна быть в формате: ИТ??-??");
        RuleFor(d => d.subgroup)
            .NotEmpty();
    }
}