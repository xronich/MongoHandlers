using FluentValidation;
using .Abstractions.Contracts.Queries.Packages.Base;
using .Abstractions.Enums.Attendees;
using .Abstractions.Exceptions.Attendees;
using .Abstractions.Exceptions.Packages;
using .Domain.Database.Context;
using .Domain.Database.Context.Entities.Attendees;
using MongoDB.Driver;

namespace .Domain.Implementation.Validators.Queries.Packages.PackageSelection
{
    public abstract class PackagesAvailableForSelectionQueryValidatorBase<TQuery> : AbstractValidator<TQuery>
        where TQuery : PackagesAvailableForSelectionQueryBase
    {
        protected abstract FilterDefinition<AttendeeBase> GetFilter(TQuery query, IDatabaseContext dbContext);

        protected PackagesAvailableForSelectionQueryValidatorBase(IDatabaseContext dbContext)
        {
            RuleFor(query => query)
                .CustomAsync(async (query, context, cancellationToken) =>
                {
                    var filterDefinition = GetFilter(query, dbContext);

                    var findOptions = new FindOptions<AttendeeBase, RegistrationStatus?>
                    {
                        Projection = Builders<AttendeeBase>.Projection.Expression(attendee =>
                            (RegistrationStatus?) attendee.RegistrationStatus)
                    };

                    var attendeeCursor =
                        await dbContext.Collection<AttendeeBase>().FindAsync(filterDefinition, findOptions);

                    using (attendeeCursor)
                    {
                        var registrationStatus = await attendeeCursor.FirstOrDefaultAsync();

                        if (registrationStatus == null)
                        {
                            throw new AttendeeWithEmailNotFoundException(query.Email);
                        }

                        if (registrationStatus != RegistrationStatus.PendingPackageSelection &&
                            registrationStatus != RegistrationStatus.RequestForPayment)
                        {
                            throw new AttendeeUnableToChoosePackageException();
                        }
                    }
                });
        }
    }
}