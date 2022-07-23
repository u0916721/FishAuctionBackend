using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using PlaXBackEnd.Models;
using PlaXBackEnd.Services;
using System.Text;
using System.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PlaXBackEnd;
using Amazon.S3;
using Amazon.Runtime;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.Configure<BookStoreDatabaseSettings>(
    builder.Configuration.GetSection("BookStoreDatabase"));
//Comment
builder.Services.AddSingleton<BooksService>();

builder.Services.AddSingleton<UserService>(); // Might need to do some seeding for a root user here.

builder.Services.AddSingleton<ListingService>();

builder.Services.AddSingleton<CliqueService>();

builder.Services.AddSingleton<MessageService>();

builder.Services.AddSingleton<FishService>();


builder.Services.AddAuthentication(opt => {
  opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://localhost:5001",
        ValidAudience = "https://localhost:5001",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your encryption here"))
      };
    });


builder.Services.AddControllers();
// In ASP.NET Core, services such as the DB context must be registered with the dependency injection (DI) container. The container provides the service to controllers.
// This is what we do below. 
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));


// AWS support.
var awsOptions = builder.Configuration.GetAWSOptions();
Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "your key here");
Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "your key here");
Environment.SetEnvironmentVariable("AWS_REGION", "us-west-1");
awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonS3>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


var mongoClient = new MongoClient(
          "your mongo DB string here");

var mongoDatabase = mongoClient.GetDatabase(
   "BookStore");

var users = mongoDatabase.GetCollection<User>(
   "Users");

var totalUsers = await users.Find(_ => true).ToListAsync();

if (totalUsers.Count == 0)
{
  User admin = new User();
  admin.Username = "Admin";
  admin.Password = SecurePasswordHasher.Hash("Password"); //change once you have accsess
  admin.Role = "Admin";
  await users.InsertOneAsync(admin);

}

app.Run();
