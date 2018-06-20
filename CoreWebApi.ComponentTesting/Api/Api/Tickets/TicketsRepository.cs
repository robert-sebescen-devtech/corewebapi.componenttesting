using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Api.Tickets
{
    public class TicketsRepository
    {
        private static string TicketsCollectionName => "tickets";

        private readonly IMongoCollection<Ticket> _collection;

        public TicketsRepository(MongoConfiguration mongoConfiguration)
        {
            var client = new MongoClient(mongoConfiguration.ConnectionString);

            _collection = client.GetDatabase(mongoConfiguration.Database).GetCollection<Ticket>(TicketsCollectionName);
        }

        internal void Create(Ticket ticket)
        {
            _collection.InsertOne(ticket);
        }

        internal Ticket Get(Guid id)
        {
            var filter = new FilterDefinitionBuilder<Ticket>()
                .Where(t => t.Id.Equals(id));

            return _collection.FindSync(filter).FirstOrDefault();
        }
    }
}
