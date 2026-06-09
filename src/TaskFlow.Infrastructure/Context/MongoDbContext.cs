using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TaskFlow.Infrastructure.Documents;
using TaskFlow.Infrastructure.Settings;

namespace TaskFlow.Infrastructure.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;

    public MongoDbContext(IMongoClient client, IOptions<MongoDbSettings> options)
    {
        _client = client;
        _database = client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoClient Client => _client;
    public IMongoCollection<JobDocument> Jobs => _database.GetCollection<JobDocument>("jobs");
    public IMongoCollection<OutboxMessageDocument> OutboxMessages => _database.GetCollection<OutboxMessageDocument>("outbox_messages");
}
