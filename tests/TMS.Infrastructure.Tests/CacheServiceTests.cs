using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using TMS.Application;
using TMS.Infrastructure;

namespace TMS.Infrastructure.Tests;

public class CacheServiceTests
{
    [Fact]
    public async Task RedisCacheService_WhenValueIsSet_CanReadDeserializedValue()
    {
        var distributedCache = new InMemoryDistributedCache();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:InstanceName"] = "TMS:"
            })
            .Build();
        var cacheService = new RedisCacheService(distributedCache, configuration);
        var value = new ProjectResponse(Guid.NewGuid(), "Project", "Description", DateTime.UtcNow, null, 0);

        await cacheService.SetAsync("project:test", value, TimeSpan.FromMinutes(1));

        var cachedValue = await cacheService.GetAsync<ProjectResponse>("project:test");
        cachedValue.Should().BeEquivalentTo(value);
    }

    [Fact]
    public async Task RedisCacheService_WhenCacheThrows_ReturnsDefaultAndDoesNotThrow()
    {
        var distributedCache = new ThrowingDistributedCache();
        var configuration = new ConfigurationBuilder().Build();
        var cacheService = new RedisCacheService(distributedCache, configuration);

        var cachedValue = await cacheService.GetAsync<ProjectResponse>("project:test");
        var setAction = async () => await cacheService.SetAsync("project:test", new { Name = "Project" });

        cachedValue.Should().BeNull();
        await setAction.Should().NotThrowAsync();
    }

    private sealed class InMemoryDistributedCache : IDistributedCache
    {
        private readonly ConcurrentDictionary<string, byte[]> _items = new();

        public byte[]? Get(string key) =>
            _items.TryGetValue(key, out var value) ? value : null;

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
            Task.FromResult(Get(key));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            _items[key] = value;

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default) =>
            Task.CompletedTask;

        public void Remove(string key) =>
            _items.TryRemove(key, out _);

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingDistributedCache : IDistributedCache
    {
        public byte[]? Get(string key) =>
            throw new InvalidOperationException("Cache is unavailable.");

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
            throw new InvalidOperationException("Cache is unavailable.");

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            throw new InvalidOperationException("Cache is unavailable.");

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) =>
            throw new InvalidOperationException("Cache is unavailable.");

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default) =>
            Task.CompletedTask;

        public void Remove(string key) =>
            throw new InvalidOperationException("Cache is unavailable.");

        public Task RemoveAsync(string key, CancellationToken token = default) =>
            throw new InvalidOperationException("Cache is unavailable.");
    }
}
