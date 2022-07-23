namespace PlaXBackEnd.Models
{
    public class TodoItem
    {
        public long Id { get; set; } // Unique key in relational database
        public string? Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
