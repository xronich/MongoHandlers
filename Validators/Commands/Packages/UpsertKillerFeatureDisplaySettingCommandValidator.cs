using FluentValidation;
using .Abstractions.Contracts.Commands.GlobalSettings;

namespace .Domain.Implementation.Validators.Commands.Packages
{
    public class UpsertKillerFeatureDisplaySettingCommandValidator
        : AbstractValidator<UpsertKillerFeatureDisplaySettingCommand>
    {
        public UpsertKillerFeatureDisplaySettingCommandValidator()
        {
            RuleFor(command => command.Text).NotEmpty();
            RuleFor(command => command.Title).NotEmpty();
            RuleFor(command => command.CheckboxLabel).NotEmpty();
        }
    }
}
