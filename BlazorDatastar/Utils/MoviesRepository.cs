using BlazorDatastar.Models;
using System.Runtime.CompilerServices;

namespace BlazorDatastar.Utils
{
    public class MoviesRepository
    {
        private readonly List<Movie> _movies = new()
            {
                new Movie { Id = 1, Title = "Inception" },
                new Movie { Id = 2, Title = "The Matrix" },
                new Movie { Id = 3, Title = "Interstellar" }
            };
        private readonly WorkDispatcher<IMovieEvent> _dispatcher;

        public MoviesRepository(WorkDispatcher<IMovieEvent> dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public Task<List<Movie>> GetMoviesAsync()
        {
            return Task.FromResult(_movies);
        }

        public async Task AddVote(int movieId, int score, Guid userId)
        {
            var movie = _movies.FirstOrDefault(m => m.Id == movieId);
            if (movie != null)
            {
                var existingVote = movie.Votes.FirstOrDefault(v => v.UserId == userId);
                if (existingVote != null)
                {
                    if(existingVote.Score == score)
                    {
                        // User is trying to vote the same score again, do nothing
                        return;
                    }
                    // Update the existing vote
                    existingVote.Score = score;
                }
                else
                {
                    // Add a new vote
                    movie.Votes.Add(new Vote { Id = movie.Votes.Count + 1, MovieId = movieId, Score = score, UserId = userId });
                }
                await _dispatcher.CreateWriter().WriteAsync(new MovieUpdated { MovieId = movieId });
            }
        }

        public async Task RemoveVote(int movieId, Guid userId)
        {
            var movie = _movies.FirstOrDefault(m => m.Id == movieId);
            if (movie is not null)
            {
                movie.RemoveVote(userId);
                await _dispatcher.CreateWriter().WriteAsync(new MovieUpdated { MovieId = movieId });
            }
        }
    }
}
