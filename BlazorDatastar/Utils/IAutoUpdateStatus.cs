
namespace BlazorDatastar.Utils
{
    internal interface IAutoUpdateStatus
    {
        bool AutoUpdate { get; }

        Task SetAutoUpdateAsync(bool value);
    }
}