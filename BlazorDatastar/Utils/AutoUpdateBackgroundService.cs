using BlazorDatastar.Models;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorDatastar.Utils
{
    public class AutoUpdateBackgroundService : BackgroundService, IAutoUpdateStatus
    {
        private readonly MoviesRepository _moviesRepository;
        private readonly WorkDispatcher<IMovieEvent> _dispatcher;
        private readonly Random _random = new();
        private int _autoUpdateFlag = 0; 

        public bool AutoUpdate
        {
            get => Interlocked.CompareExchange(ref _autoUpdateFlag, 0, 0) == 1;
        }

        public async Task SetAutoUpdateAsync(bool value)
        {
            var newValue = value ? 1 : 0;
            var oldValue = Interlocked.Exchange(ref _autoUpdateFlag, newValue);

            if (oldValue != newValue) // Value has changed
            {
                await _dispatcher.CreateWriter().WriteAsync(new AutoUpdateChangedEvent { AutoUpdate = value });
            }
        }

        public AutoUpdateBackgroundService(MoviesRepository moviesRepository, WorkDispatcher<IMovieEvent> dispatcher)
        {
            _moviesRepository = moviesRepository;
            _dispatcher = dispatcher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (AutoUpdate)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        var movies = await _moviesRepository.GetMoviesAsync();
                        if (movies.Count > 0)
                        {
                            var randomMovie = movies[_random.Next(movies.Count)];
                            var randomScore = _random.Next(1, 6); // Random score between 1 and 5
                            await _moviesRepository.AddVote(randomMovie.Id, randomScore, Guid.NewGuid());
                        }

                        await Task.Delay(2000, stoppingToken); // Wait for 2 seconds
                    }

                    await SetAutoUpdateAsync(false); // Reset AutoUpdate flag
                }

                await Task.Delay(500, stoppingToken); // Poll every 500ms
            }
        }
    }
}
