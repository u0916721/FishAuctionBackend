using PlaXBackEnd.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace PlaXBackEnd.Services
{
  public class CliqueService
  {
    private readonly IMongoCollection<Clique> posts;

    public CliqueService(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
      var mongoClient = new MongoClient(
          bookStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = mongoClient.GetDatabase(
          bookStoreDatabaseSettings.Value.DatabaseName);

      posts = mongoDatabase.GetCollection<Clique>(
          bookStoreDatabaseSettings.Value.CliqueCollectionName);
    }

    public async Task<List<Clique>> GetAsync() =>
        await posts.Find(_ => true).ToListAsync();

    public async Task<Clique?> GetAsync(string id) =>
        await posts.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Clique?> GetByUsername(string cliqueName) =>
      await posts.Find(x => x.Name == cliqueName).FirstOrDefaultAsync();

    public async Task CreateAsync(Clique newClique) =>
        await posts.InsertOneAsync(newClique);

    public async Task UpdateAsync(string id, Clique updatedClique) =>
        await posts.ReplaceOneAsync(x => x.Id == id, updatedClique);

    public async Task RemoveAsync(string id) =>
        await posts.DeleteOneAsync(x => x.Id == id);

    public async Task RemoveByUserNameAsync(string name) =>
       await posts.DeleteOneAsync(x => x.Name == name);
  }
}

