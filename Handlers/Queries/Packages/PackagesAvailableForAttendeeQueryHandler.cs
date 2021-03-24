using .Abstractions.Contracts.Queries.Packages;
using .Domain.Database.Context;
using .Domain.Database.Context.Entities.Attendees;
using .Domain.Implementation.Handlers.Queries.Packages.Base;

namespace .Domain.Implementation.Handlers.Queries.Packages
{
    public class PackagesAvailableForAttendeeQueryHandler
        : PackagesAvailableForSelectionQueryHandlerBase<PackagesAvailableForAttendeeQuery>
    {
        public PackagesAvailableForAttendeeQueryHandler(IDatabaseContext dbContext) : base(dbContext)
        {
        }

        protected override string Discriminator => nameof(Attendee);
    }
}