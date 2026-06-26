using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using TMS.Application;

namespace TMS.Infrastructure
{
    public class RedisCacheService : ICacheService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly string _instanceName;
        private readonly TimeSpan _operationTimeout;

        public RedisCacheService(IDistributedCache cache, IConfiguration configuration)
        {
            _cache = cache;
            _configuration = configuration;
            _instanceName = configuration["Redis:InstanceName"] ?? string.Empty;
            _operationTimeout = TimeSpan.FromMilliseconds(
                Convert.ToInt32(configuration["Redis:OperationTimeoutMilliseconds"] ?? "500"));
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedValue = await _cache
                    .GetStringAsync(key, cancellationToken)
                    .WaitAsync(_operationTimeout, cancellationToken);

                return string.IsNullOrWhiteSpace(cachedValue)
                    ? default
                    : JsonSerializer.Deserialize<T>(cachedValue, SerializerOptions);
            }
            catch
            {
                return default;
            }
        }

        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpirationRelativeToNow = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value, SerializerOptions);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5)
                };

                await _cache
                    .SetStringAsync(key, serializedValue, options, cancellationToken)
                    .WaitAsync(_operationTimeout, cancellationToken);
            }
            catch
            {
                // Cache failures should not break the primary SQL-backed flow.
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache
                    .RemoveAsync(key, cancellationToken)
                    .WaitAsync(_operationTimeout, cancellationToken);
            }
            catch
            {
                // Cache failures should not break the primary SQL-backed flow.
            }
        }

        public async Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration["Redis:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            try
            {
                var options = ConfigurationOptions.Parse(connectionString);
                options.AbortOnConnectFail = false;
                options.ConnectRetry = 1;
                options.ConnectTimeout = (int)_operationTimeout.TotalMilliseconds;
                options.SyncTimeout = (int)_operationTimeout.TotalMilliseconds;
                options.AsyncTimeout = (int)_operationTimeout.TotalMilliseconds;

                using var connection = await ConnectionMultiplexer
                    .ConnectAsync(options)
                    .WaitAsync(_operationTimeout, cancellationToken);

                var database = connection.GetDatabase();
                var redisKeyPattern = $"{_instanceName}{keyPrefix}*";

                foreach (var endpoint in connection.GetEndPoints())
                {
                    var server = connection.GetServer(endpoint);
                    if (!server.IsConnected)
                        continue;

                    foreach (var redisKey in server.Keys(pattern: redisKeyPattern))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await database.KeyDeleteAsync(redisKey);
                    }
                }
            }
            catch
            {
                // Prefix invalidation is best-effort; stale entries expire by TTL.
            }
        }
    }
}
