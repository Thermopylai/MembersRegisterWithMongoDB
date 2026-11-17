using MembersRegisterWithMongoDB.Models;
using MongoDB.Driver;

namespace MembersRegisterWithMongoDB.Services;

public class MemberService : IDisposable
{
    private MongoClient? client;
    private IMongoCollection<Member>? collection;

    public async Task<List<Member>> ConnectAsync(string connectionString)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
        MongoClient? nextClient = new MongoClient(settings);
        try
        {
            var nextCollection = nextClient.GetDatabase("jasenrekisteri").GetCollection<Member>("jasenet");
            var members = await ReadAsync(nextCollection);
            var previousClient = client;
            client = nextClient;
            nextClient = null;
            collection = nextCollection;
            previousClient?.Dispose();
            return members;
        }
        finally
        {
            nextClient?.Dispose();
        }
    }

    public Task<List<Member>> GetAllAsync() => ReadAsync(GetCollection());

    public Task AddAsync(Member member) => GetCollection().InsertOneAsync(member);

    public async Task<bool> UpdateAsync(Member member)
    {
        var update = Builders<Member>.Update
            .Set(item => item.FirstName, member.FirstName)
            .Set(item => item.LastName, member.LastName)
            .Set(item => item.Address, member.Address)
            .Set(item => item.PostalCode, member.PostalCode)
            .Set(item => item.Phone, member.Phone)
            .Set(item => item.Email, member.Email)
            .Set(item => item.MembershipStartDate, member.MembershipStartDate);
        var result = await GetCollection().UpdateOneAsync(item => item.Id == member.Id, update);
        return result.MatchedCount == 1;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await GetCollection().DeleteOneAsync(item => item.Id == id);
        return result.DeletedCount == 1;
    }

    private IMongoCollection<Member> GetCollection() => collection
        ?? throw new InvalidOperationException("MongoDB-yhteyttä ei ole muodostettu.");

    private static Task<List<Member>> ReadAsync(IMongoCollection<Member> source) => source
        .Find(FilterDefinition<Member>.Empty)
        .SortBy(member => member.LastName).ThenBy(member => member.FirstName)
        .ToListAsync();

    public void Dispose() => client?.Dispose();
}