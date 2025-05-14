using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using StarFederation.Datastar;
using StarFederation.Datastar.DependencyInjection;
namespace BlazorDatastar.Utils
{
    public interface IDatastarBlazorService : IDatastarServerSentEventService
    {
        // Define methods and properties for the service here
        Task MergeComponentAsFragementAsync<TComponent>(Dictionary<string, object?> parameters, ServerSentEventMergeFragmentsOptions options) where TComponent : IComponent;
    }

    public class DatastarBlazorService : IDatastarBlazorService
    {
        private readonly IDatastarServerSentEventService _serverSentEventService;
        private readonly HtmlRenderer _htmlRenderer;

        public DatastarBlazorService(IDatastarServerSentEventService serverSentEventService, HtmlRenderer htmlRenderer)
        {
            _serverSentEventService = serverSentEventService;
            _htmlRenderer = htmlRenderer;
        }

        public async Task MergeComponentAsFragementAsync<TComponent>(Dictionary<string, object?> parameters, ServerSentEventMergeFragmentsOptions options) where TComponent : IComponent
        {
            var html = await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var pv = ParameterView.FromDictionary(parameters);
                var output = await _htmlRenderer.RenderComponentAsync<TComponent>(pv);

                return output.ToHtmlString();
            });

            await MergeFragmentsAsync(html, options);
        }

        public ISendServerEvent Handler => _serverSentEventService.Handler;

        public Task MergeFragmentsAsync(string fragment) =>
            _serverSentEventService.MergeFragmentsAsync(fragment);

        public Task MergeFragmentsAsync(string fragment, ServerSentEventMergeFragmentsOptions options) =>
            _serverSentEventService.MergeFragmentsAsync(fragment, options);

        public Task RemoveFragmentsAsync(string selector) =>
            _serverSentEventService.RemoveFragmentsAsync(selector);

        public Task RemoveFragmentsAsync(string selector, ServerSentEventRemoveFragmentsOptions options) =>
            _serverSentEventService.RemoveFragmentsAsync(selector, options);

        public Task MergeSignalsAsync(string dataSignals) =>
            _serverSentEventService.MergeSignalsAsync(dataSignals);

        public Task MergeSignalsAsync(string dataSignals, ServerSentEventMergeSignalsOptions options) =>
            _serverSentEventService.MergeSignalsAsync(dataSignals, options);

        public Task RemoveSignalsAsync(IEnumerable<string> paths) =>
            _serverSentEventService.RemoveSignalsAsync(paths);

        public Task RemoveSignalsAsync(IEnumerable<string> paths, ServerSentEventOptions options) =>
            _serverSentEventService.RemoveSignalsAsync(paths, options);

        public Task ExecuteScriptAsync(string script) =>
            _serverSentEventService.ExecuteScriptAsync(script);

        public Task ExecuteScriptAsync(string script, ServerSentEventExecuteScriptOptions options) =>
            _serverSentEventService.ExecuteScriptAsync(script, options);
    }
}
