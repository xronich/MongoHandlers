using System.Linq;
using System.Threading.Tasks;
using CQRS.Abstractions.Buses;
using CQRS.Abstractions.Exceptions.Events;
using CQRS.Abstractions.Handlers;
using Extensions.FrameworkTypes;
using Abstractions.Contracts.Events.Packages;
using Abstractions.Contracts.Queries.Attendees;
using Domain.Database.Context;
using Domain.Database.Context.Entities.Attendees;
using Domain.Database.Context.Entities.Packages;
using MongoDB.Driver;

namespace Domain.Implementation.Handlers.Events.Packages
{
    public class PackagePriceUpdatedEventSubscriber : EventSubscriber<PackageUpdatedEvent>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IQueryBus _queryBus;

        public PackagePriceUpdatedEventSubscriber(IDatabaseContext databaseContext, IQueryBus queryBus)
        {
            _databaseContext = databaseContext;
            _queryBus = queryBus;
        }

        public override async Task OnEvent(PackageUpdatedEvent @event)
        {
            var attendeesIdsResponse =
                await _queryBus.Execute<RegularAttendeesIdsByPackageIdQuery, RegularAttendeesIdsByPackageIdResponse>(
                    new RegularAttendeesIdsByPackageIdQuery
                    {
                        PackageId = @event.PackageId
                    });

            if (attendeesIdsResponse.AttendeesIds.IsNull() || !attendeesIdsResponse.AttendeesIds.Any())
            {
                return;
            }

            var packageModelProjection = Builders<Package>.Projection.Expression(package => new PackageModel
            {
                Price = package.Price
            });

            var packagesFilterDefinition = Builders<Package>.Filter.Eq(package => package.Id, @event.PackageId);

            var packageModelsCursor = await _databaseContext.Collection<Package>()
                .FindAsync(packagesFilterDefinition, new FindOptions<Package, PackageModel>
                {
                    Projection = packageModelProjection
                });

            PackageModel packageModel;
            using (packageModelsCursor)
            {
                packageModel = await packageModelsCursor.FirstOrDefaultAsync();
            }

            if (packageModel.IsNull())
            {
                throw new EntityNotFoundException<Package, PackageUpdatedEvent>(@event);
            }

            var attendeesFilterDefinition =
                Builders<Attendee>.Filter.In(attendee => attendee.Id, attendeesIdsResponse.AttendeesIds);

            var updateDefinition =
                Builders<Attendee>.Update.Set(attendee => attendee.Package.Price, packageModel.Price);

            await _databaseContext.Collection<Attendee>().UpdateManyAsync(attendeesFilterDefinition, updateDefinition);
        }

        private class PackageModel
        {
            public decimal? Price { get; set; }
        }
    }
}