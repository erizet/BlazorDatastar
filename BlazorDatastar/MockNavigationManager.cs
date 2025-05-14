using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorDatastar
{
    public class MockNavigationManager : NavigationManager, IHostEnvironmentNavigationManager
    {
        bool _isInitialized = false;
        public MockNavigationManager()
        {
            if (_isInitialized)
                return;
            Initialize("http://localhost/", "http://localhost/");
            _isInitialized = true;
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            // No-op for string rendering
        }

        void IHostEnvironmentNavigationManager.Initialize(string baseUri, string uri)
        {
            Initialize(baseUri, uri);
        }
    }
}