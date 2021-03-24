using System;
using System.Linq.Expressions;
using .Abstractions.Contracts.Queries.Packages;
using .Domain.Database.Context;
using .Domain.Database.Context.Entities.Attendees;
using MongoDB.Driver;

namespace .Domain.Implementation.Validators.Queries.Packages.PackageSelection
{
    public class PackagesAvailableForAttendeeQueryValidator
        : PackagesAvailableForSelectionQueryValidatorBase<PackagesAvailableForAttendeeQuery>
    {
        public PackagesAvailableForAttendeeQueryValidator(IDatabaseContext dbContext) : base(dbContext)
        {
        }

        protected override FilterDefinition<AttendeeBase> GetFilter(PackagesAvailableForAttendeeQuery query,
            IDatabaseContext dbContext)
        {
            return Builders<AttendeeBase>.Filter.Where(
                attendee => attendee is Attendee && attendee.Email == query.Email);
        }
    }
}