using System.Linq;
using Extensions.FrameworkTypes;
using FluentValidation;
using Abstractions.Contracts.Commands.Packages;
using Abstractions.Enums.Packages;
using Domain.Database.Context;
using Domain.Database.Context.Entities.PredefinedActivities;
using Domain.Implementation.Validators.Commands.PackageTemplateKeysValidator;
using MongoDB.Driver;

namespace Domain.Implementation.Validators.Commands.Packages
{
    public class CreatePackageCommandValidator : AbstractValidator<CreatePackageCommand>
    {
        public CreatePackageCommandValidator(IDatabaseContext databaseContext,
            IValidator<PackageEmailTemplateValidationModel> templateKeysValidator)
        {
            RuleFor(command => command.Name).NotEmpty();

            RuleFor(command => command.SubTitle).NotEmpty().When(command => command.SubTitle.IsNotNull());

            RuleFor(command => command.Title).NotEmpty();

            RuleFor(command => command.Type).NotNull();

            RuleFor(command => command.AvailableAttendeeType).NotNull();

            RuleFor(command => command.MessagesLimit).NotNull().GreaterThanOrEqualTo(valueToCompare: 0);

            RuleFor(command => command.PredefinedActivitiesIds)
                .NotEmpty();

            RuleForEach(command => command.PredefinedActivitiesIds)
                .Matches("^[0-9a-fA-F]{24}$")
                .DependentRules(() =>
                {
                    RuleFor(command => command.PredefinedActivitiesIds)
                        .MustAsync(async (command, predefinedActivitiesIds, cancellationToken) =>
                            await databaseContext.Collection<PredefinedActivity>()
                                .CountAsync(activity => command.PredefinedActivitiesIds.Contains(activity.Id)) ==
                            command.PredefinedActivitiesIds.Count)
                        .WithErrorCode("create-package-0001")
                        .WithMessage("All predefined activities must exist");
                })
                .When(command => command.PredefinedActivitiesIds?.Any() ?? false);

            RuleFor(command => command.Price)
                .GreaterThan(valueToCompare: 0)
                .When(command => command.Type == PackageType.Paid ||
                command.Type == PackageType.Contract);

            RuleFor(command => command.Price)
                .Must(price => price == null)
                .When(command => command.Type == PackageType.Free);

            RuleFor(command => command.LastModifiedAdminId).Matches("^[0-9a-fA-F]{24}$");
            RuleFor(command => command.KillerFeatureDisplaySetting.GalleryItemId)
                .Matches("^[0-9a-fA-F]{24}$")
                .When(command => command.KillerFeatureDisplaySetting.IsNotNull());

            RuleFor(command => command.Status).NotNull();

            RuleFor(command => command.Visibility).NotNull();

            ValidateTemplateKeys(templateKeysValidator);
        }

        private void ValidateTemplateKeys(IValidator<PackageEmailTemplateValidationModel> templateKeysValidator)
        {
            RuleFor(command => new PackageEmailTemplateValidationModel
                {
                    TemplateKey = command.MultiUserConfirmationEmailTemplateKey
                })
                .SetValidator(templateKeysValidator);

            RuleFor(command => new PackageEmailTemplateValidationModel
                {
                    TemplateKey = command.MultiUserFailedPaymentEmailTemplateKey
                })
                .SetValidator(templateKeysValidator);

            RuleFor(command => new PackageEmailTemplateValidationModel
                {
                    TemplateKey = command.ConfirmationEmailTemplateName
                })
                .SetValidator(templateKeysValidator);

            RuleFor(command => new PackageEmailTemplateValidationModel
                {
                    TemplateKey = command.FailedPaymentEmailTemplateName
                })
                .SetValidator(templateKeysValidator);
        }
    }
}