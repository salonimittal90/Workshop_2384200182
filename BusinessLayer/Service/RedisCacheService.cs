using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLayer.Service
{
    public class RedisCacheService
    {
        private readonly IDatabase _cacheDb;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _cacheDb = redis.GetDatabase();
        }

        public async Task SetDataAsync<T>(string key, T value, TimeSpan expiry)
        {
            var jsonData = JsonSerializer.Serialize(value);
            await _cacheDb.StringSetAsync(key, jsonData, expiry);
        }

        public async Task<T> GetDataAsync<T>(string key)
        {
            var value = await _cacheDb.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value) : default;
        }

        public async Task RemoveDataAsync(string key)
        {
            await _cacheDb.KeyDeleteAsync(key);
        }
    }
}