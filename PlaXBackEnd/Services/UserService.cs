using PlaXBackEnd.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace PlaXBackEnd.Services
{
  public class UserService
  {

    private readonly IMongoCollection<User> users;

    public UserService(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
      var mongoClient = new MongoClient(
          bookStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = mongoClient.GetDatabase(
          bookStoreDatabaseSettings.Value.DatabaseName);

      users = mongoDatabase.GetCollection<User>(
          bookStoreDatabaseSettings.Value.UserCollectionName);
    }

    public async Task<List<User>> GetAsync() =>
        await users.Find(_ => true).ToListAsync();

    public async Task<User?> GetAsync(string id) =>
        await users.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByUsername(string username) =>
    await users.Find(x => x.Username == username).FirstOrDefaultAsync();

    public async Task CreateAsync(User newBook) =>
        await users.InsertOneAsync(newBook);

    public async Task UpdateAsync(string id, User updatedBook) =>
        await users.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await users.DeleteOneAsync(x => x.Id == id);


  }
}
