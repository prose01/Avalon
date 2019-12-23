using Avalon.Model;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Avalon.Data
{
    public class ProfileContext
    {
        private readonly IMongoDatabase _database = null;

        public ProfileContext(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<AbstractProfile> Profiles
        {
            get
            {
                return _database.GetCollection<AbstractProfile>("Profile");
            }
        }
    }
}
