using PlaXBackEnd.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace PlaXBackEnd.Services
{
  public class FishService
  {

    private readonly IMongoCollection<Fish> fishes;

    public FishService(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
      var mongoClient = new MongoClient(
          bookStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = mongoClient.GetDatabase(
          bookStoreDatabaseSettings.Value.DatabaseName);

      fishes = mongoDatabase.GetCollection<Fish>(
          bookStoreDatabaseSettings.Value.FishCollectionName);
    }

    public async Task<List<Fish>> GetAsync() =>
        await fishes.Find(_ => true).ToListAsync();

    public async Task<Fish?> GetAsync(string id) =>
        await fishes.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Fish?> GetByName(string name) =>
    await fishes.Find(x => x.Name == name).FirstOrDefaultAsync();

    public async Task CreateAsync(Fish newFish) =>
        await fishes.InsertOneAsync(newFish);

    public async Task UpdateAsync(string id, Fish updatedFish) =>
        await fishes.ReplaceOneAsync(x => x.Id == id, updatedFish);

    public async Task RemoveAsync(string id) =>
        await fishes.DeleteOneAsync(x => x.Id == id);

    public async Task RemoveByNameAsync(string name) =>
       await fishes.DeleteOneAsync(x => x.Name == name);
  }
}
