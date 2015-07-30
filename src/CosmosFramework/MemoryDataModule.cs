using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Redlock.CSharp;
using StackExchange.Redis;

namespace Cosmos
{
    public class MemoryData
    {
        private ConnectionMultiplexer _redis;

        internal MemoryData(ConnectionMultiplexer redis)
        {
            _redis = redis;
        }
    }
    public class MemoryDataModule
    {
        private ConnectionMultiplexer redis;
        public IDatabase Db;
        private Redlock.CSharp.Redlock _redlock;
        public MemoryDataModule()
        {
            redis = ConnectionMultiplexer.Connect("127.0.0.1");
            _redlock = new Redlock.CSharp.Redlock(redis);
            Db = redis.GetDatabase();
        }

        public void UsingLock(string key, Action action)
        {
            Lock locker;
            while (!CheckLock(key, out locker))
            {
                // blocking
            }

            try
            {
                action();

            }
            finally
            {
                _redlock.Unlock(locker);
            }
            
        }
        public string GetLockToken(string key)
        {
            return "LOCKOLOCKOLOCO_" + key;
        }

        public bool CheckLock(string key, out Lock locker)
        {
            var lockKey = GetLockToken(key);
            return _redlock.Lock(lockKey, new TimeSpan(0, 0, 10), out locker);
        }

        public async Task<bool> Set(string key, string value)
        {
            lock (_cache)
            {
                _cache.Remove(key);    
            }
            
            var rValue = await Db.StringSetAsync(key, value);
            lock (_cache)
            {
                if (_cache.ContainsKey(key))
                    _cache[key] = value;
            }
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

        public Lock Lock(string key)
        {
            Lock locker;
            while (!CheckLock(key, out locker))
            {
                // blocking
            }
            return locker;
        }
        public void UnLock(Lock locker)
        {
            _redlock.Unlock(locker);
        }
    }
}
