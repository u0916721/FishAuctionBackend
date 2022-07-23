using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace PlaXBackEnd.Models
{
    //The database context is the main class that coordinates Entity Framework functionality for a data model.
    //This class is created by deriving from the Microsoft.EntityFrameworkCore.DbContext class.
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; } = null!;
    }
}
