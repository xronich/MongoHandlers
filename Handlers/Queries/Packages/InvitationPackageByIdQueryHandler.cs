using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.Abstractions.Exceptions.Queries;
using Extensions.FrameworkTypes;
using Abstractions.Contracts.Queries.Packages;
using Domain.Database.Context;
using Domain.Database.Context.Entities.Packages;
using Domain.Database.Handlers;
using Domain.Implementation.JsonQueries;
using Domain.Implementation.JsonQueries.Serializer;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Domain.Implementation.Handlers.Queries.Packages
{
    public class InvitationPackageByIdQueryHandler
        : ContextQueryHandlerBase<InvitationPackageByIdQuery, InvitationPackageByIdResponse>
    {
        private readonly IJsonPipelineSerializer _pipelineSerializer;

        public InvitationPackageByIdQueryHandler(IDatabaseContext dbContext, IJsonPipelineSerializer pipelineSerializer) : base(dbContext)
        {
            _pipelineSerializer = pipelineSerializer;
        }

        private ICollection<BsonDocument> GetJsonQuery(string id)
        {
            var json = MongoJsonQueries.InvitationPackagesQuery;

            var query = new StringBuilder(json).Replace("{0}", id).ToString();

            var bsonDocuments = _pipelineSerializer.Serialize(query);

            return bsonDocuments;
        }

        public override async Task<InvitationPackageByIdResponse> Execute(InvitationPackageByIdQuery query)
        {
            var jsonQuery = GetJsonQuery(query.Id);

            var pipelineDefinition = PipelineDefinition<InvitationPackage, InvitationPackage>
                .Create(jsonQuery);

            var request = await DbContext.Collection<InvitationPackage>().AggregateAsync(pipelineDefinition);

            InvitationPackage model;

            using (request)
            {
                model = await request.FirstOrDefaultAsync();

                if (model.IsNull())
                {
                    throw new EntityNotFoundException<InvitationPackage, InvitationPackageByIdQuery,
                        InvitationPackageByIdResponse>(query);
                }
            }

            var response = new InvitationPackageByIdResponse
            {
                Name = model.Name,
                Id = model.Id,
                Title = model.Title,
                UpdateDate = model.UpdateDate,
                AvailableAttendeeType = model.AvailableAttendeeType,
                ConfirmationEmailTemplateName = model.ConfirmationEmailTemplateName,
                CreationDate = model.CreationDate,
                FailedPaymentEmailTemplateName = model.FailedPaymentEmailTemplateName,
                LastModifiedAdminId = model.LastModifiedAdminId,
                MessagesLimit = model.MessagesLimit,
                MultiUserConfirmationEmailTemplateKey = model.MultiUserConfirmationEmailTemplateKey,
                MultiUserFailedPaymentEmailTemplateKey = model.MultiUserFailedPaymentEmailTemplateKey,
                OriginalPackageId = model.OriginalPackageId,
                PredefinedActivities = model.PredefinedActivities,
                Price = model.Price,
                Status = model.Status,
                SubTitle = model.SubTitle,
                Type = model.Type
            };

            return response;
        }
    }
}