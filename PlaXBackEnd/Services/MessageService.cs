using PlaXBackEnd.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
namespace PlaXBackEnd.Services
{
  public class MessageService
  {

    private readonly IMongoCollection<Message> messages;

    public MessageService(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
      var mongoClient = new MongoClient(
          bookStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = mongoClient.GetDatabase(
          bookStoreDatabaseSettings.Value.DatabaseName);

      messages = mongoDatabase.GetCollection<Message>(
          bookStoreDatabaseSettings.Value.MessageCollectionName);
    }

    public async Task<List<Message>> GetAsync() =>
        await messages.Find(_ => true).ToListAsync();

    public async Task<Message?> GetAsync(string id) =>
        await messages.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Message?> GetByTagName(string tagName) =>
      await messages.Find(x => x.Tag == tagName).FirstOrDefaultAsync();

    public async Task CreateAsync(Message newBook) =>
        await messages.InsertOneAsync(newBook);

    public async Task UpdateAsync(string id, Message updatedBook) =>
        await messages.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await messages.DeleteOneAsync(x => x.Id == id);
  }
}
