using .Abstractions.Services.MongoDB;
using MongoDB.Bson;

namespace .Domain.Implementation.Services.MongoDB
{
    public class ObjectIdGenerationService : IObjectIdGenerationService
    {
        public string GenerateObjectId()
        {
            return ObjectId.GenerateNewId().ToString();
        }
    }
}