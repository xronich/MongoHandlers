using System.Collections.Generic;
using System.Linq;
using .Abstractions.Contracts.Queries.Packages;
using .Abstractions.Enums.Packages;
using .Domain.Database.Context;
using .Domain.Database.Context.Entities.Attendees;
using .Domain.Implementation.Handlers.Queries.Packages.Base;
using MongoDB.Bson;
using MongoDB.Driver;
using MoreLinq.Extensions;

namespace .Domain.Implementation.Handlers.Queries.Packages
{
    public class PackagesAvailableForCryptoTraderQueryHandler
        : PackagesAvailableForSelectionQueryHandlerBase<PackagesAvailableForCryptoTraderQuery>
    {
        public PackagesAvailableForCryptoTraderQueryHandler(IDatabaseContext dbContext) : base(dbContext)
        {
        }

        protected override string Discriminator => nameof(CryptoTrader);

        protected override PipelineDefinition<AttendeeBase, Package> GetPipelineDefinition(PackagesAvailableForCryptoTraderQuery query)
        {
            var pipeline = base.GetPipelineDefinition(query);

            var matchDefinition = pipeline.Match(package => package.Type == PackageType.Free);

            return matchDefinition;
        }
    }
}