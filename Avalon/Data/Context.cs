using Avalon.Model;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Avalon.Data
{
    public class Context
    {
        private readonly IMongoDatabase _database = null;

        public Context(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<CurrentUser> CurrentUser => _database.GetCollection<CurrentUser>("Profile");
        public IMongoCollection<Profile> Profiles => _database.GetCollection<Profile>("Profile");

        public IMongoCollection<Feedback> Feedbacks => _database.GetCollection<Feedback>("Feedback");
    }
}
