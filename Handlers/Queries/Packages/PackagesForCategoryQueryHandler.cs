using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using .Abstractions.Contracts.Queries.Packages;
using .Abstractions.Enums;
using .Abstractions.Enums.Packages;
using .Domain.Database.Context;
using .Domain.Database.Context.Entities;
using .Domain.Database.Handlers;
using .Domain.Implementation.JsonQueries;
using .Domain.Implementation.JsonQueries.Serializer;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace .Domain.Implementation.Handlers.Queries.Packages
{
    public class PackagesForCategoryQueryHandler
        : ContextQueryHandlerBase<PackagesForCategoryQuery,
            PackagesResponse>
    {
        private readonly IJsonPipelineSerializer _pipelineSerializer;

        public PackagesForCategoryQueryHandler(IDatabaseContext dbContext, IJsonPipelineSerializer pipelineSerializer) : base(dbContext)
        {
            _pipelineSerializer = pipelineSerializer;
        }

        private IEnumerable<BsonDocument> GetJsonQuery(string categoryId)
        {
            var json = MongoJsonQueries.PackagesForCategoryQuery;
            var query = new StringBuilder(json).Replace("{0}", categoryId).ToString();

            var bsonDocuments = _pipelineSerializer.Serialize(query);

            return bsonDocuments;
        }

        public override async Task<PackagesResponse> Execute(PackagesForCategoryQuery query)
        {
            var jsonQuery = GetJsonQuery(query.CategoryId);

            var pipeline = PipelineDefinition<Category, Package>.Create(jsonQuery);

            var asyncCursor = await DbContext.Collection<Category>().AggregateAsync(pipeline);

            var packages = await asyncCursor.ToListAsync();

            return new PackagesResponse
            {
                Packages = packages.Select(package => new PackagesResponse.Package
                    {
                        Id = package.Id,
                        PredefinedActivities = package.PredefinedActivities,
                        Price = package.Price,
                        CreationDate = package.CreationDate,
                        Type = package.Type,
                        Name = package.Name,
                        Title = package.Title,
                        SubTitle = package.SubTitle,
                        Visibility = package.Visibility,
                        MessagesLimit = package.MessagesLimit,
                        AvailableAttendeeType = package.AvailableAttendeeType,
                        KillerFeatureDisplaySetting = package.KillerFeatureDisplaySettings == null
                            ? null
                            : new PackagesResponse.Package.KillerFeatureDisplaySettings()
                            {
                                MetadataObject = package.KillerFeatureDisplaySettings.MetadataObject,
                                Title = package.KillerFeatureDisplaySettings.Title,
                                GalleryItemId = package.KillerFeatureDisplaySettings.GalleryItemId,
                                Descriptions = new PackagesResponse.Package.KillerFeatureDescription
                                {
                                    FirstColumn = new PackagesResponse.Package.KillerFeatureDescriptionColumn
                                    {
                                        Subtitle = package.KillerFeatureDisplaySettings.Descriptions?.FirstColumn
                                            ?.Subtitle,
                                        Description =
                                            package.KillerFeatureDisplaySettings.Descriptions?.FirstColumn?.Description
                                    },
                                    SecondColumn = new PackagesResponse.Package.KillerFeatureDescriptionColumn
                                    {
                                        Subtitle =
                                            package.KillerFeatureDisplaySettings.Descriptions?.SecondColumn?.Subtitle,
                                        Description =
                                            package.KillerFeatureDisplaySettings.Descriptions?.SecondColumn?.Description
                                    },
                                    ThirdColumn = new PackagesResponse.Package.KillerFeatureDescriptionColumn
                                    {
                                        Subtitle = package.KillerFeatureDisplaySettings.Descriptions?.ThirdColumn
                                            ?.Subtitle,
                                        Description =
                                            package.KillerFeatureDisplaySettings.Descriptions?.ThirdColumn?.Description
                                    }
                                }
                            }
                    })
                    .ToList()
            };
        }

        [BsonIgnoreExtraElements]
        public class Package
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }

            public string Name { get; set; }
            public string Title { get; set; }
            public string SubTitle { get; set; }
            public PackageType Type { get; set; }
            public PackageVisibility Visibility { get; set; }
            public int MessagesLimit { get; set; }
            public decimal? Price { get; set; }
            public DateTime CreationDate { get; set; }
            public AttendeeType AvailableAttendeeType { get; set; }

            [BsonRepresentation(BsonType.ObjectId)]
            public ICollection<string> PredefinedActivities { get; set; }

            public KillerFeatureDisplaySetting KillerFeatureDisplaySettings { get; set; }

            [BsonIgnoreExtraElements]
            public class KillerFeatureDisplaySetting
            {
                public string Title { get; set; }
                public KillerFeatureDescription Descriptions { get; set; }

                [BsonRepresentation(BsonType.ObjectId)]
                public string GalleryItemId { get; set; }

                [BsonIgnore]
                public JObject MetadataObject { get; set; } = new JObject();

                [JsonIgnore]
                public BsonDocument Metadata
                {
                    get => BsonDocument.Parse(MetadataObject.ToString());
                    set
                    {
                        var jsonWriterSettings = JsonWriterSettings.Defaults.Clone();
                        jsonWriterSettings.OutputMode = JsonOutputMode.Strict;
                        MetadataObject = JObject.Parse(value.ToJson(jsonWriterSettings));
                    }
                }
            }

            [BsonIgnoreExtraElements]
            public class KillerFeatureDescription
            {
                public KillerFeatureDescriptionColumn FirstColumn { get; set; }
                public KillerFeatureDescriptionColumn SecondColumn { get; set; }
                public KillerFeatureDescriptionColumn ThirdColumn { get; set; }
            }

            [BsonIgnoreExtraElements]
            public class KillerFeatureDescriptionColumn
            {
                public string Subtitle { get; set; }
                public string Description { get; set; }
            }
        }
    }
}