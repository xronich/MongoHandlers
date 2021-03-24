using FluentValidation;
using .Abstractions.Contracts.Queries.Packages;

namespace .Domain.Implementation.Validators.Queries.Packages
{
    public class PackageByIdQueryValidator : AbstractValidator<PackageByIdQuery>
    {
        public PackageByIdQueryValidator()
        {
            RuleFor(query => query.Id).Matches("^[0-9a-fA-F]{24}$");
        }
    }
}