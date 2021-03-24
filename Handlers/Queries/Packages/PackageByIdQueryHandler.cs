using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.Abstractions.Exceptions.Queries;
using Extensions.FrameworkTypes;
using Abstractions.Contracts.Queries.Packages;
using Domain.Database.Context;
using Domain.Database.Context.Entities.Packages;
using Domain.Database.Context.Entities.PredefinedActivities;
using Domain.Database.Handlers;
using Domain.Implementation.JsonQueries;
using Domain.Implementation.JsonQueries.Serializer;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Domain.Implementation.Handlers.Queries.Packages
{
    public class PackageByIdQueryHandler : ContextQueryHandlerBase<PackageByIdQuery, PackageResponse>
    {
        private readonly IJsonPipelineSerializer _pipelineSerializer;

        public PackageByIdQueryHandler(IDatabaseContext dbContext, IJsonPipelineSerializer pipelineSerializer) : base(dbContext)
        {
            _pipelineSerializer = pipelineSerializer;
        }

        private ICollection<BsonDocument> GetJsonQuery(string id)
        {
            var json = MongoJsonQueries.PackagesQuery;
            var query = new StringBuilder(json).Replace("{0}",id).ToString();
            var bsonDocuments = _pipelineSerializer.Serialize(query);
            return bsonDocuments;
        }

        public override async Task<PackageResponse> Execute(PackageByIdQuery query)
        {
            var jsonQuery = GetJsonQuery(query.Id);

            var pipelineDefinition = PipelineDefinition<Package, PackageModel>.Create(jsonQuery);

            var request = await DbContext.Collection<Package>().AggregateAsync(pipelineDefinition);

            PackageModel packageModel;

            using (request)
            {
                packageModel = await request.FirstOrDefaultAsync();

                if (packageModel.IsNull())
                {
                    throw new EntityNotFoundException<Package, PackageByIdQuery, PackageResponse>(query);
                }
            }

            return new PackageResponse
            {
                Name = packageModel.Name,
                Title = packageModel.Title,
                SubTitle = packageModel.SubTitle,
                AvailableAttendeeType = packageModel.AvailableAttendeeType,
                ConfirmationEmailTemplateName = packageModel.ConfirmationEmailTemplateName,
                Id = packageModel.Id,
                FailedPaymentEmailTemplateName = packageModel.FailedPaymentEmailTemplateName,
                LastModifiedAdminId = packageModel.LastModifiedAdminId,
                MessagesLimit = packageModel.MessagesLimit,
                Type = packageModel.Type,
                Status = packageModel.Status,
                Visibility = packageModel.Visibility,
                Price = packageModel.Price,
                UpdateDate = packageModel.UpdateDate,
                PredefinedActivities = packageModel.Activities?.Select(activity =>
                                               new PackageResponse.PredefinedActivity
                                               {
                                                   Id = activity.Id,
                                                   ImageUrl = activity.ImageUrl,
                                                   Description = activity.Description,
                                                   Title = activity.Title,
                                                   Activity = activity.Activity
                                               })
                                           .ToList() ?? new List<PackageResponse.PredefinedActivity>(),
                Categories = packageModel.Categories?.Select(category => new PackagesPageResponse.Package.Category
                {
                    Id = category.Id,
                    Description = category.Description,
                    Image = category.Image,
                    Name = category.Name,
                    Status = category.Status
                }).ToList(),
                CreationDate = packageModel.CreationDate,
                MultiUserConfirmationEmailTemplateKey = packageModel.MultiUserConfirmationEmailTemplateKey,
                MultiUserFailedPaymentEmailTemplateKey = packageModel.MultiUserFailedPaymentEmailTemplateKey,
                KillerFeatureDisplaySetting = packageModel.KillerFeatureDisplaySettings == null
                    ? null
                    : new PackageResponse.KillerFeatureDisplaySettings
                    {
                        GalleryItemId = packageModel.KillerFeatureDisplaySettings.GalleryItemId,
                        GalleryItem = packageModel.GalleryItem?.Select(galery => new PackageResponse.Gallery
                        {
                            Width = galery.Width,
                            Height = galery.Height,
                            ImagesUrls = galery.ImagesUrls,
                            Alt = galery.Alt,
                            Title = galery.Title,
                            Categories = galery.Categories
                        }).FirstOrDefault(),
                        MetadataObject = packageModel.KillerFeatureDisplaySettings.MetadataObject,
                        Title = packageModel.KillerFeatureDisplaySettings.Title,
                        Descriptions = new PackageResponse.KillerFeatureDescription
                        {
                            FirstColumn = new PackageResponse.KillerFeatureDescriptionColumn
                            {
                                Subtitle = packageModel.KillerFeatureDisplaySettings?.Descriptions?.FirstColumn
                                    ?.Subtitle,
                                Description = packageModel.KillerFeatureDisplaySettings?.Descriptions?.FirstColumn
                                    ?.Description
                            },
                            SecondColumn = new PackageResponse.KillerFeatureDescriptionColumn
                            {
                                Subtitle = packageModel.KillerFeatureDisplaySettings?.Descriptions?.SecondColumn
                                    ?.Subtitle,
                                Description = packageModel.KillerFeatureDisplaySettings?.Descriptions?.SecondColumn
                                    ?.Description
                            },
                            ThirdColumn = new PackageResponse.KillerFeatureDescriptionColumn
                            {
                                Subtitle = packageModel.KillerFeatureDisplaySettings?.Descriptions?.ThirdColumn
                                    ?.Subtitle,
                                Description = packageModel.KillerFeatureDisplaySettings?.Descriptions?.ThirdColumn
                                    ?.Description
                            }
                        }
                    }
            };
        }

        private class PackageModel : Package
        {
            public ICollection<Database.Context.Entities.Galleries.Gallery> GalleryItem { get; set; }
            public ICollection<PredefinedActivity> Activities { get; set; }
            public ICollection<Database.Context.Entities.Category> Categories { get; set; }
        }
    }
}