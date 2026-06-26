using TMS.Application;

namespace TMS.Infrastructure
{
    public class NoOpCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult<T?>(default);

        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
