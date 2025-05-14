namespace BlazorDatastar.Models
{
    public interface IMovieEvent
    {
    }

    public record MovieDeleted : IMovieEvent
    {
        public int MovieId { get; init; }
    }

    public record MovieAdded : IMovieEvent
    {
        public int MovieId { get; init; }
    }
    public record MovieUpdated : IMovieEvent
    {
        public int MovieId { get; init; }
    }

    public record AutoUpdateChangedEvent : IMovieEvent
    {
        public bool AutoUpdate { get; init; }
    }
}
