using .Abstractions.Contracts.Queries.Packages;
using .Domain.Database.Context;
using .Domain.Database.Context.Entities.Attendees;
using MongoDB.Driver;

namespace .Domain.Implementation.Validators.Queries.Packages.PackageSelection
{
    public class PackagesAvailableForCryptoTraderQueryValidator
        : PackagesAvailableForSelectionQueryValidatorBase<PackagesAvailableForCryptoTraderQuery>
    {
        public PackagesAvailableForCryptoTraderQueryValidator(IDatabaseContext dbContext) : base(dbContext)
        {
        }

        protected override FilterDefinition<AttendeeBase> GetFilter(PackagesAvailableForCryptoTraderQuery query,
            IDatabaseContext dbContext)
        {
            return Builders<AttendeeBase>.Filter.Where(
                attendee => attendee is CryptoTrader && attendee.Email == query.Email);
        }
    }
}