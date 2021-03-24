using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using .Abstractions.Contracts.Queries.Packages;
using .Abstractions.Contracts.Queries.Packages.Base;
using .Abstractions.Enums.Packages;
using .Domain.Database.Context;
using .Domain.Database.Context.Entities.Attendees;
using .Domain.Database.Handlers;
using .Domain.Implementation.JsonQueries;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AttendeeType = .Abstractions.Enums.AttendeeType;

namespace .Domain.Implementation.Handlers.Queries.Packages.Base
{
    public abstract class PackagesAvailableForSelectionQueryHandlerBase<TQuery>
        : ContextQueryHandlerBase<TQuery, PackagesResponse> where TQuery : PackagesAvailableForSelectionQueryBase
    {
        protected PackagesAvailableForSelectionQueryHandlerBase(IDatabaseContext dbContext) : base(dbContext)
        {
        }

        protected abstract string Discriminator { get; }

        protected virtual PipelineDefinition<AttendeeBase, Package> GetPipelineDefinition(TQuery query)
        {
            var json = MongoJsonQueries.PackagesForAttendeeQuery;

            var formattedQuery = new StringBuilder(json)
                .Replace("{0}", query.Email)
                .Replace("{1}", Discriminator)
                .ToString();

            var stages = JArray.Parse(formattedQuery).Select(token => BsonDocument.Parse(token.ToString())).ToList();

            return PipelineDefinition<AttendeeBase, Package>.Create(stages);
        }

        public override async Task<PackagesResponse> Execute(TQuery query)
        {
            var pipeline = GetPipelineDefinition(query);

            var asyncCursor = await DbContext.Collection<AttendeeBase>().AggregateAsync(pipeline);

            var packages = await asyncCursor.ToListAsync();

            return new PackagesResponse
            {
                Packages = packages.Select(package => new PackagesResponse.Package
                    {
                        Id = package.Id,
                        PredefinedActivities = package.PredefinedActivities,
                        Price = package.Type == PackageType.Contract ? null : package.Price,
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

            public PackagesForCategoryQueryHandler.Package.KillerFeatureDisplaySetting KillerFeatureDisplaySettings
            {
                get;
                set;
            }

            [BsonIgnoreExtraElements]
            public class KillerFeatureDisplaySetting
            {
                public string Title { get; set; }
                public PackagesForCategoryQueryHandler.Package.KillerFeatureDescription Descriptions { get; set; }

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
                public PackagesForCategoryQueryHandler.Package.KillerFeatureDescriptionColumn FirstColumn { get; set; }
                public PackagesForCategoryQueryHandler.Package.KillerFeatureDescriptionColumn SecondColumn { get; set; }
                public PackagesForCategoryQueryHandler.Package.KillerFeatureDescriptionColumn ThirdColumn { get; set; }
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