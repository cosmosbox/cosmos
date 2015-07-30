using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Cosmos
{
    public class MemoryData
    {
        private ConnectionMultiplexer redis;
        public IDatabase Db;
        public MemoryData()
        {
            redis = ConnectionMultiplexer.Connect("127.0.0.1");

            Db = redis.GetDatabase();
        }
        public async Task<bool> Set(string key, string value)
        {
            lock (_cache)
            {
                _cache.Remove(key);
            }
            

            var rValue = await Db.StringSetAsync(key, value);

            return rValue;
        }
        public Dictionary<string, RedisValue> _cache = new Dictionary<string, RedisValue>();

        public async Task<RedisValue> Get(string key)
        {
            RedisValue rValue;
            lock (_cache)
            {
                if (_cache.TryGetValue(key, out rValue))
                {
                    return rValue;
                }

            }

            rValue = await Db.StringGetAsync(key);
            _cache[key] = rValue;

            return rValue;

        }
    }
}
