using Avalon.Model;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Avalon.Data
{
    public class Context
    {
        private readonly IMongoDatabase _database = null;

        public Context(IConfiguration config)
        {
            var client = new MongoClient(config.GetValue<string>("Mongo_ConnectionString"));
            if (client != null)
                _database = client.GetDatabase(config.GetValue<string>("Mongo_Database"));
        }

        public IMongoCollection<CurrentUser> CurrentUser => _database.GetCollection<CurrentUser>("Profile");
        public IMongoCollection<Profile> Profiles => _database.GetCollection<Profile>("Profile");

        public IMongoCollection<Feedback> Feedbacks => _database.GetCollection<Feedback>("Feedback");

        public IMongoCollection<GroupModel> Groups => _database.GetCollection<GroupModel>("ChatGroups");
    }
}
