using BlazorDatastar.Components;
using BlazorDatastar.Components.Partials;
using BlazorDatastar.Models;
using BlazorDatastar.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using StarFederation.Datastar.DependencyInjection;
using StarFederation.Datastar.ModelBinding;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Xml.Linq;

namespace BlazorDatastar
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents();
            builder.Services.AddDatastar().AddDatastarMvc();
            builder.Services.AddScoped<IDatastarBlazorService, DatastarBlazorService>();
            builder.Services.AddScoped<HtmlRenderer>();
            builder.Services.AddSingleton<MoviesRepository>();
            builder.Services.AddSingleton<Program.MySignals>();
            builder.Services.AddSingleton<WorkDispatcher<IMovieEvent>>();
            builder.Services.AddSingleton<AutoUpdateBackgroundService>();
            builder.Services.AddSingleton<IAutoUpdateStatus>(sp => sp.GetRequiredService<AutoUpdateBackgroundService>());
            builder.Services.AddHostedService(sp => sp.GetRequiredService<AutoUpdateBackgroundService>());

            //builder.Services.AddSingleton<NavigationManager, MockNavigationManager>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.UseSession();

            app.Use(async (context, next) =>
            {
                var userId = CookieHelper.GetOrCreateUserId(context);
                context.Items["UserId"] = userId; // Store the userId in HttpContext.Items
                await next.Invoke();
            });

            app.MapStaticAssets();
            app.MapRazorComponents<App>();

            app.MapPost("/post", async (IDatastarBlazorService dbs, [FromForm] string name, [FromForm] int amount) =>
            {
                for (int i = 1; i <= Math.Min(10, amount); i++)
                {
                    await dbs.MergeComponentAsFragementAsync<Counter>(
                        new Dictionary<string, object?> { { nameof(Counter.Value), i } },
                        new ServerSentEventMergeFragmentsOptions()
                        {
                            Selector = "#edit",
                            MergeMode = StarFederation.Datastar.FragmentMergeMode.Inner
                        }
                    );

                    await Task.Delay(500);
                }
                await dbs.MergeFragmentsAsync($"""<div id="edit"><h1>Saved</h1><span>{name}, {amount}</div>""");
            });

            app.MapPost("/vote/{movieid:int}", async (IDatastarBlazorService dbs, MoviesRepository repo, HttpContext context, [FromRoute] int movieid, [FromForm] int score) =>
            {
                var userId = CookieHelper.GetOrCreateUserId(context); // Read or create the user ID
                await repo.AddVote(movieid, score, Guid.Parse(userId)); // Pass the user ID to AddVoteAsync
            }).DisableAntiforgery();

            app.MapDelete("/vote/{movieid:int}", async (IDatastarBlazorService dbs, MoviesRepository repo, HttpContext context, [FromRoute] int movieid) =>
            {
                var userId = CookieHelper.GetOrCreateUserId(context); // Read or create the user ID
                await repo.RemoveVote(movieid, Guid.Parse(userId));
            }).DisableAntiforgery();

            app.MapGet("/movielist", async (IDatastarBlazorService dbs, WorkDispatcher<IMovieEvent> dispatcher, MoviesRepository movieRepo, IAutoUpdateStatus autoStatus, CancellationToken ct) =>
            {
                async Task SendAutoUpdate(bool autoUpdate)
                {
                    await dbs.MergeSignalsAsync(JsonSerializer.Serialize(new MySignals() { AutoUpdate = autoUpdate }));
                }
                async Task SendList(int[] updatedMovies)
                {
                    var movies = (await movieRepo.GetMoviesAsync()).Select(m => (m, updatedMovies.Contains(m.Id)));
                    await dbs.MergeComponentAsFragementAsync<MovieTable>(
                        new Dictionary<string, object?> { { nameof(MovieTable.Movies), movies } },
                        new ServerSentEventMergeFragmentsOptions()
                        {
                            MergeMode = StarFederation.Datastar.FragmentMergeMode.Morph
                        }
                    );
                }

                await SendList([]);
                await SendAutoUpdate(autoStatus.AutoUpdate);

                await using var reader = dispatcher.CreateReader();
                await foreach (var upd in reader.ReadAllAsync(ct))
                {
                    if (upd is MovieUpdated movieUpdated)
                    {
                        await SendList([movieUpdated.MovieId]);
                    }
                    else if (upd is AutoUpdateChangedEvent evt)
                    {
                        await SendAutoUpdate(autoStatus.AutoUpdate);
                    }
                }
            });

            app.MapPost("/autoupdate", async (IDatastarBlazorService dbs, WorkDispatcher<IMovieEvent> dispatcher, IAutoUpdateStatus autoStatus, [FromBody] MySignals data) => //   IDatastarSignalsReaderService signalsReader) =>
            {
                //var json = await signalsReader.ReadSignalsAsync();
                //Console.WriteLine($"json =  {json}");
                //var data = await signalsReader.ReadSignalsAsync<MySignals>();

                await autoStatus.SetAutoUpdateAsync(data.AutoUpdate);
                Console.WriteLine($"autoUpdate =  {data.AutoUpdate}");
            });

            app.Run();
        }

        private class MySignals
        {
            [JsonPropertyName("autoUpdate")]
            public bool AutoUpdate { get; set; }
        }
    }
}
