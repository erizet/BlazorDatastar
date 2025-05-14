namespace BlazorDatastar.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int Score { get; set; } // Score range: 1-5
        public Guid UserId { get; set; } // Unique identifier for the user
    }
}
