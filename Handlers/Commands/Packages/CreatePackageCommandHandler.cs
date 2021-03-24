using System;
using System.Threading.Tasks;
using CQRS.Abstractions.Buses;
using Abstractions.Contracts.Commands.Packages;
using Domain.Database.Context;
using Domain.Database.Context.Entities.Packages;
using Domain.Database.Context.Entities.Packages.ComplexTypes;
using Domain.Database.Handlers;

namespace Domain.Implementation.Handlers.Commands.Packages
{
    public class CreatePackageCommandHandler : ContextCommandHandlerBase<CreatePackageCommand>
    {
        public CreatePackageCommandHandler(IDatabaseContext dbContext, IEventBus eventBus) : base(eventBus, dbContext)
        {
        }

        public override async Task Execute(CreatePackageCommand command)
        {
            var package = new Package
            {
                Name = command.Name,
                Title = command.Title,
                SubTitle = command.SubTitle,
                MessagesLimit = command.MessagesLimit,
                Price = command.Price,
                Status = command.Status,
                Visibility = command.Visibility,
                Type = command.Type,
                ConfirmationEmailTemplateName = command.ConfirmationEmailTemplateName,
                FailedPaymentEmailTemplateName = command.FailedPaymentEmailTemplateName,
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                LastModifiedAdminId = command.LastModifiedAdminId,
                AvailableAttendeeType = command.AvailableAttendeeType,
                PredefinedActivities = command.PredefinedActivitiesIds,
                MultiUserConfirmationEmailTemplateKey = command.MultiUserConfirmationEmailTemplateKey,
                MultiUserFailedPaymentEmailTemplateKey = command.MultiUserFailedPaymentEmailTemplateKey,
                KillerFeatureDisplaySettings = command.KillerFeatureDisplaySetting == null
                    ? null
                    : new KillerFeatureDisplaySetting
                    {
                        MetadataObject = command.KillerFeatureDisplaySetting.MetadataObject,
                        Descriptions = new KillerFeatureDescription
                        {
                            FirstColumn = new KillerFeatureDescriptionColumn
                            {
                                Description =
                                    command.KillerFeatureDisplaySetting.Descriptions?.FirstColumn?.Description,
                                Subtitle = command.KillerFeatureDisplaySetting.Descriptions?.FirstColumn?.Subtitle
                            },
                            SecondColumn = new KillerFeatureDescriptionColumn
                            {
                                Description =
                                    command.KillerFeatureDisplaySetting.Descriptions?.SecondColumn?.Description,
                                Subtitle = command.KillerFeatureDisplaySetting.Descriptions?.SecondColumn?.Subtitle
                            },
                            ThirdColumn = new KillerFeatureDescriptionColumn
                            {
                                Description =
                                    command.KillerFeatureDisplaySetting.Descriptions?.ThirdColumn?.Description,
                                Subtitle = command.KillerFeatureDisplaySetting.Descriptions?.ThirdColumn?.Subtitle
                            }
                        },
                        Title = command.KillerFeatureDisplaySetting.Title,
                        GalleryItemId = command.KillerFeatureDisplaySetting.GalleryItemId
                    }
            };

            await DbContext.Collection<Package>().InsertOneAsync(package);
        }
    }
}