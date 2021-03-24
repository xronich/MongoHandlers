using System.Threading.Tasks;
using .Abstractions.Loggers;
using log4net;
using Newtonsoft.Json.Linq;

namespace .Domain.Implementation.Loggers
{
    public class AdminActionLogger : IAdminActionLogger
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(AdminActionLogger));

        public Task LogAdminAction(AdminActionModel model)
        {
            var adminActionObject = JObject.FromObject(model);

            _logger.Info(adminActionObject);

            return Task.CompletedTask;
        }
    }
}