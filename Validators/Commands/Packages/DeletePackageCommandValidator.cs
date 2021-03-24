using FluentValidation;
using .Abstractions.Contracts.Commands.Packages;

namespace .Domain.Implementation.Validators.Commands.Packages
{
    public class DeletePackageCommandValidator : AbstractValidator<DeletePackageCommand>
    {
        public DeletePackageCommandValidator()
        {
            RuleFor(package => package.Id).NotEmpty();
        }
    }
}