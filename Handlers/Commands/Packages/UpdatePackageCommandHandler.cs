using System;
using System.Threading.Tasks;
using CQRS.Abstractions.Buses;
using Abstractions.Contracts.Commands.Packages;
using Abstractions.Contracts.Events.Packages;
using Domain.Database.Context;
using Domain.Database.Context.Entities.Packages;
using Domain.Database.Context.Entities.Packages.ComplexTypes;
using Domain.Database.Handlers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Domain.Implementation.Handlers.Commands.Packages
{
    public class UpdatePackageCommandHandler : ContextCommandHandlerBase<UpdatePackageCommand>
    {
        public UpdatePackageCommandHandler(IDatabaseContext dbContext, IEventBus eventBus) : base(eventBus,
            dbContext)
        {
        }

        public override async Task Execute(UpdatePackageCommand command)
        {
            var filter = Builders<Package>.Filter.Eq(package => package.Id, command.Id);

            var updateDefinition = Builders<Package>.Update
                .Set(package => package.Name, command.Name)
                .Set(package => package.Title, command.Title)
                .Set(package => package.SubTitle, command.SubTitle)
                .Set(package => package.Price, command.Price)
                .Set(package => package.Status, command.Status)
                .Set(package => package.Visibility, command.Visibility)
                .Set(package => package.Type, command.Type)
                .Set(package => package.MessagesLimit, command.MessagesLimit)
                .Set(package => package.UpdateDate, DateTime.UtcNow)
                .Set(package => package.LastModifiedAdminId, command.LastModifiedAdminId)
                .Set(package => package.ConfirmationEmailTemplateName, command.ConfirmationEmailTemplateName)
                .Set(package => package.FailedPaymentEmailTemplateName, command.FailedPaymentEmailTemplateName)
                .Set(package => package.AvailableAttendeeType, command.AvailableAttendeeType)
                .Set(package => package.PredefinedActivities, command.PredefinedActivities)
                .Set(package => package.MultiUserConfirmationEmailTemplateKey,
                    command.MultiUserConfirmationEmailTemplateKey)
                .Set(package => package.MultiUserFailedPaymentEmailTemplateKey,
                    command.MultiUserFailedPaymentEmailTemplateKey);

            if (command.KillerFeatureDisplaySetting != null)
            {
                var killerFeatureDescription = command.KillerFeatureDisplaySetting == null
                    ? null
                    : new KillerFeatureDescription
                    {
                        FirstColumn = new KillerFeatureDescriptionColumn
                        {
                            Subtitle = command.KillerFeatureDisplaySetting.Descriptions?.FirstColumn?.Subtitle,
                            Description = command.KillerFeatureDisplaySetting.Descriptions?.FirstColumn?.Description
                        },
                        SecondColumn = new KillerFeatureDescriptionColumn
                        {
                            Subtitle = command.KillerFeatureDisplaySetting.Descriptions?.SecondColumn?.Subtitle,
                            Description = command.KillerFeatureDisplaySetting.Descriptions?.SecondColumn?.Description
                        },
                        ThirdColumn = new KillerFeatureDescriptionColumn
                        {
                            Subtitle = command.KillerFeatureDisplaySetting.Descriptions?.ThirdColumn?.Subtitle,
                            Description = command.KillerFeatureDisplaySetting.Descriptions?.ThirdColumn?.Description
                        }
                    };

                var killerFeatureDisplaySetting = new KillerFeatureDisplaySetting
                {
                    Descriptions = killerFeatureDescription,
                    GalleryItemId = command.KillerFeatureDisplaySetting.GalleryItemId,
                    Metadata = BsonDocument.Parse(command.KillerFeatureDisplaySetting.MetadataObject.ToString()),
                    Title = command.KillerFeatureDisplaySetting.Title
                };

                updateDefinition = updateDefinition
                    .Set(package => package.KillerFeatureDisplaySettings,
                        killerFeatureDisplaySetting);
            }

            await DbContext.Collection<Package>().UpdateOneAsync(filter, updateDefinition);

            await Raise(command, new PackageUpdatedEvent
            {
                PackageId = command.Id
            });
        }
    }
}