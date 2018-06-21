using MongoDB.Driver;

namespace Api.Configuration
{
    public class MongoConfiguration
    {
        public string ConnectionString { get; }

        public string Database { get; }

        public MongoConfiguration(string connectionString, string database)
        {
            ConnectionString = connectionString;
            Database = database;
        }
    }
}