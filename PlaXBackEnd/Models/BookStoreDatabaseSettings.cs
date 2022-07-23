namespace PlaXBackEnd.Models
{
    //The preceding BookStoreDatabaseSettings class is used to store the appsettings.json file's BookStoreDatabase property values.
    //The JSON and C# property names are named identically to ease the mapping process.
    public class BookStoreDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string BooksCollectionName { get; set; } = null!;

        public string UserCollectionName { get; set; } = null!;

        public string ListingCollectionName { get; set; } = null!;

        public string CliqueCollectionName { get; set; } = null!;

        public string MessageCollectionName { get; set; } = null!;

        public string FishCollectionName { get; set; } = null!;
  }
}
