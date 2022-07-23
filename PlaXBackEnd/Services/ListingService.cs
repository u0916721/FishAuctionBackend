using PlaXBackEnd.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
namespace PlaXBackEnd.Services
{
  public class ListingService
  {

    private readonly IMongoCollection<Listing> posts;

    public ListingService(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
      var mongoClient = new MongoClient(
          bookStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = mongoClient.GetDatabase(
          bookStoreDatabaseSettings.Value.DatabaseName);

      posts = mongoDatabase.GetCollection<Listing>(
          bookStoreDatabaseSettings.Value.ListingCollectionName);
    }

    public async Task<List<Listing>> GetAsync() =>
        await posts.Find(_ => true).ToListAsync();

    public async Task<Listing?> GetAsync(string id) =>
        await posts.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<List<Listing>> GetByUsername(string sellerUserName) =>
      await posts.Find(x => x.SellerUserName == sellerUserName).ToListAsync();
   
    public async Task CreateAsync(Listing newBook) =>
        await posts.InsertOneAsync(newBook);

    public async Task UpdateAsync(string id, Listing updatedBook) =>
        await posts.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await posts.DeleteOneAsync(x => x.Id == id);
  }
}
