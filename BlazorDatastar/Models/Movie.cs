namespace BlazorDatastar.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<Vote> Votes { get; set; } = new();

        public double AverageScore => Votes.Any() ? Votes.Average(v => v.Score) : 0.0;

        public bool HasUserVoted(Guid userId)
        {
            return Votes.Any(v => v.UserId == userId);
        }
        public void RemoveVote(Guid userId)
        {
            var vote = Votes.FirstOrDefault(v => v.UserId == userId);
            if (vote != null)
            {
                Votes.Remove(vote);
            }
        }
    }
}
