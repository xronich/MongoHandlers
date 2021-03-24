using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using .Abstractions.Contracts.Queries.Packages;
using .Abstractions.Enums.Packages;
using .Domain.Database.Context.Entities.Packages;
using .Domain.Database.Context.Entities.PredefinedActivities;
using .Domain.Database.Context;
using .Domain.Database.Handlers;
using MongoDB.Driver;
using MoreLinq;
using MoreLinq.Extensions;

namespace .Domain.Implementation.Handlers.Queries.Packages
{
    public class PublicPackagesQueryHandler : ContextQueryHandlerBase<PublicPackagesQuery, PublicPackagesResponse>
    {
        public PublicPackagesQueryHandler(IDatabaseContext dbContext) : base(dbContext)
        {
        }

        public override async Task<PublicPackagesResponse> Execute(PublicPackagesQuery query)
        {
            var packageWithPredefinedActivities = await DbContext.Collection<Package>()
                .Aggregate()
                .Match(p => p.Status == PackageStatus.Enabled)
                .Lookup<Package, PredefinedActivity, PackageWithPredefinedActivities>(
                    DbContext.Collection<PredefinedActivity>(),
                    package => package.PredefinedActivities,
                    activity => activity.Id,
                    model => model.Activities)
                .ToListAsync();

            var packages = packageWithPredefinedActivities
                .Select(pack => new PublicPackagesResponse.Package
                {
                    Name = pack.Name,
                    Title = pack.Title,
                    Type = pack.Type,
                    Price = pack.Type == PackageType.Contract ? null : pack.Price,
                    MessagesLimit = pack.MessagesLimit,
                    AvailableAttendeeType = pack.AvailableAttendeeType,
                    PredefinedActivities = pack.Activities?.OrderBy(x => x.ActivityDisplayLocation).Select(s => s.Title).ToList()
                })
                .ToList();

            return new PublicPackagesResponse
            {
                Packages = packages
            };
        }

        public class PackageWithPredefinedActivities : Package
        {
            public ICollection<PredefinedActivity> Activities { get; set; }
        }

    }
}