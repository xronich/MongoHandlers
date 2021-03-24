using FluentValidation;
using FluentValidation.Results;
using .Abstractions.Contracts.Commands.Packages;
using .Abstractions.Enums.Packages;
using .Abstractions.Exceptions.Packages;
using .Domain.Database.Context;
using .Domain.Database.Context.Entities.Packages;
using .Domain.Implementation.Extensions;
using .Domain.Implementation.Validators.Commands.PackageTemplateKeysValidator;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace .Domain.Implementation.Validators.Commands.Packages
{
    public class UpdatePackageCommandValidator : AbstractValidator<UpdatePackageCommand>
    {
        private readonly string _query = JsonQueries.MongoJsonQueries.PackageExistenceAndCategoryAssignmentQuery;
        public UpdatePackageCommandValidator(IDatabaseContext dbContext,
            IValidator<PackageEmailTemplateValidationModel> templateKeysValidator)
        {
            RuleFor(package => package).NotEmpty().CustomAsync(async (package, customContext, arg3) =>
            {
                var pipelineDefinition = MongoPipelineDefinitionExtensions.CreateFromJsonQuery<Package, PackageExistenceModel>(_query);
                var definition = pipelineDefinition.Match(model => model.Id == package.Id);
                var cursor = await dbContext.Collection<Package>().AggregateAsync(definition);

                using (cursor)
                {
                    var packageToCheck = await cursor.FirstOrDefaultAsync();

                    if (packageToCheck == null)
                    {
                        customContext.AddFailure(new ValidationFailure(nameof(package.Id), "Package not found")
                        {
                            ErrorCode = "update-package-0001"
                        });
                        return;
                    }

                    if (packageToCheck.IsAssignedToCategory && package.Visibility == PackageVisibility.Public)
                    {
                        throw new PackageCannotBecomePublicException(package.Id);
                    }
                }
            });
            ValidateTemplateKeys(templateKeysValidator);
        }

        private void ValidateTemplateKeys(IValidator<PackageEmailTemplateValidationModel> templateKeysValidator)
        {
            RuleFor(command => new PackageEmailTemplateValidationModel
            {
                TemplateKey = command.MultiUserConfirmationEmailTemplateKey
            }).SetValidator(templateKeysValidator);

            RuleFor(command => new PackageEmailTemplateValidationModel
                {
                    TemplateKey = command.MultiUserFailedPaymentEmailTemplateKey
                })
                .SetValidator(templateKeysValidator)
                .When(command => !string.IsNullOrEmpty(command.MultiUserFailedPaymentEmailTemplateKey));
        }

        private class PackageExistenceModel
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            public bool IsAssignedToCategory { get; set; }
        }
    }
}