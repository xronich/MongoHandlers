using System.Threading.Tasks;
using CQRS.Abstractions.Buses;
using CQRS.Abstractions.Exceptions.Commands;
using Abstractions.Contracts.Commands.Packages;
using Domain.Database.Context;
using Domain.Database.Context.Entities;
using Domain.Database.Context.Entities.Packages;
using Domain.Database.Handlers;
using MongoDB.Driver;

namespace Domain.Implementation.Handlers.Commands.Packages
{
    public class DeletePackageCommandHandler : ContextCommandHandlerBase<DeletePackageCommand>
    {
        public DeletePackageCommandHandler(IDatabaseContext dbContext, IEventBus eventBus) : base(eventBus, dbContext)
        {
        }

        public override async Task Execute(DeletePackageCommand command)
        {
            var deletedPackage = await DbContext.Collection<Package>()
                .FindOneAndDeleteAsync(package => package.Id == command.Id);

            if (deletedPackage == null)
            {
                throw new EntityNotFoundException<Package, DeletePackageCommand>(command);
            }

            var categoryFilterDefinition =
                Builders<Category>.Filter.AnyEq(category => category.Packages, command.Id);

            var categoryUpdateDefinition =
                Builders<Category>.Update.Pull(category => category.Packages, command.Id);

            await DbContext.Collection<Category>().UpdateManyAsync(categoryFilterDefinition, categoryUpdateDefinition);
        }
    }
}