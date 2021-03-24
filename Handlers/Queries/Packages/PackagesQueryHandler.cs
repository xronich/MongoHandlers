using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using .Abstractions.Contracts.Queries.DynamicFiltering;
using .Abstractions.Contracts.Queries.Packages;
using .Abstractions.Enums;
using .Domain.Database.Context;
using .Domain.Implementation.Handlers.Queries.DynamicFiltering;
using .Domain.Implementation.JsonQueries;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Package = .Domain.Database.Context.Entities.Packages.Package;

namespace .Domain.Implementation.Handlers.Queries.Packages
{
    public class PackagesQueryHandler : DynamicFilteringQueryHandlerBase<PackagesQueryHandler.PackageResultModel, PackagesQueryHandler.PackageResultModel,
            PackagesQuery, PackagesPageResponse>
    {
        public PackagesQueryHandler(IDatabaseContext dbContext) : base(dbContext)
        {
        }


        private static IEnumerable<BsonDocument> GetJsonQuery()
        {
            var json = MongoJsonQueries.PackagesBackOfficeQuery;

            var stages = JArray.Parse(json).Select(token => BsonDocument.Parse(token.ToString())).ToList();
            return stages;
        }

        protected override Task<IAsyncCursor<PackageResultModel>> GetCursorAsync(FilterDefinition<PackageResultModel> filterDefinition, SortDefinition<PackageResultModel> sortDefinition, int skip, int take)
        {
            var stages = GetJsonQuery();

            var pipelineDefinition =
                PipelineDefinition<PackageResultModel, PackageResultModel>
                    .Create(stages);

            var sortingDefinition = Builders<PackageResultModel>.Sort.Combine(sortDefinition,
                Builders<PackageResultModel>.Sort.Ascending(model => model.Id));

            var matchPipelineDefinition = pipelineDefinition.Match(filterDefinition);
            var sortingPipelineDefinition = matchPipelineDefinition.Sort(sortingDefinition);
            var skipPipelineDefinition = sortingPipelineDefinition.Skip(skip);
            var limitPipelineDefinition = skipPipelineDefinition.Limit(take);

            return DbContext.Collection<PackageResultModel>()
                .AggregateAsync(limitPipelineDefinition);
        }

        protected override async Task<long> GetTotalElements(
            FilterDefinition<PackageResultModel> filterDefinition)
        {
            var stages = GetJsonQuery();

            var pipelineDefinition =
                PipelineDefinition<PackageResultModel, PackageResultModel>
                    .Create(stages);

            var matchPipelineDefinition = pipelineDefinition.Match(filterDefinition);

            var countDefinition = matchPipelineDefinition.Count();

            var countCursor = await DbContext.Collection<PackageResultModel>()
                .AggregateAsync(countDefinition);

            using (countCursor)
            {
                var aggregateCountResult = await countCursor.FirstOrDefaultAsync();
                return aggregateCountResult?.Count ?? default(long);
            }
        }

        protected override async Task<PackagesPageResponse> ProcessCursor(IAsyncCursor<PackageResultModel> cursor, int totalPages,
            long totalFilteredItems)
        {
            List<PackageResultModel> packageModels;

            using (cursor)
            {
                packageModels = await cursor.ToListAsync();
            }

            var packagesResponse = new PackagesPageResponse
            {
                TotalItems = totalFilteredItems,
                TotalPages = totalPages,
                Packages = packageModels.Select(model => new PackagesPageResponse.Package
                    {
                        Id = model.Id,
                        Name = model.Name,
                        Title = model.Title,
                        SubTitle = model.SubTitle,
                        Status = model.Status,
                        Type = model.Type,
                        Visibility = model.Visibility,
                        AvailableAttendeeType = model.AvailableAttendeeType,
                        ConfirmationEmailTemplateName = model.ConfirmationEmailTemplateName,
                        MessagesLimit = model.MessagesLimit,
                        LastModifiedAdminId = model.LastModifiedAdminId,
                        FailedPaymentEmailTemplateName = model.FailedPaymentEmailTemplateName,
                        Price = model.Price,
                        CreationDate = model.CreationDate,
                        UpdateDate = model.UpdateDate,
                        PredefinedActivities = model.PredefinedActivities,
                        UsageCount = model.UsageCount,
                        MultiUserConfirmationEmailTemplateKey = model.MultiUserConfirmationEmailTemplateKey,
                        MultiUserFailedPaymentEmailTemplateKey = model.MultiUserFailedPaymentEmailTemplateKey,
                        KillerFeatureDisplaySetting = model.KillerFeatureDisplaySettings == null
                            ? null
                            : new PackagesPageResponse.Package.KillerFeatureDisplaySettings
                            {
                                GalleryItemId = model.KillerFeatureDisplaySettings.GalleryItemId,
                                MetadataObject = model.KillerFeatureDisplaySettings.MetadataObject,
                                Title = model.KillerFeatureDisplaySettings.Title,
                                Descriptions = new PackagesPageResponse.Package.KillerFeatureDescription
                                {
                                    FirstColumn = new PackagesPageResponse.Package.KillerFeatureDescriptionColumn
                                    {
                                        Subtitle = model.KillerFeatureDisplaySettings.Descriptions?.FirstColumn?.Subtitle,
                                        Description = model.KillerFeatureDisplaySettings.Descriptions?.FirstColumn?.Description
                                    },
                                    SecondColumn = new PackagesPageResponse.Package.KillerFeatureDescriptionColumn
                                    {
                                        Subtitle = model.KillerFeatureDisplaySettings.Descriptions?.SecondColumn?.Subtitle,
                                        Description = model.KillerFeatureDisplaySettings.Descriptions?.SecondColumn?.Description
                                    },
                                    ThirdColumn = new PackagesPageResponse.Package.KillerFeatureDescriptionColumn
                                    {
                                        Subtitle = model.KillerFeatureDisplaySettings.Descriptions?.ThirdColumn?.Subtitle,
                                        Description = model.KillerFeatureDisplaySettings.Descriptions?.ThirdColumn?.Description
                                    }
                                }
                            },
                        Categories = model.Categories?.Select(category => new PackagesPageResponse.Package.Category
                        {
                            Id = category.Id,
                            Description = category.Description,
                            Image = category.Image,
                            Name = category.Name,
                            Status = category.Status
                        }).ToList()
                    })
                    .ToList()
            };

            return packagesResponse;
        }

        public class PackageResultModel : Package, IDynamicFilteringProjection
        {
            public int UsageCount { get; set; }
            public ICollection<Category> Categories { get; set; }
        }

        public class Category
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Image { get; set; }
            public CategoryStatus Status { get; set; }
        }
    }
}