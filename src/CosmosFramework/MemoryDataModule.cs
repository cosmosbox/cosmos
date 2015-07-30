using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MsgPack.Serialization;
using Redlock.CSharp;
using StackExchange.Redis;

namespace Cosmos
{
    public class MemoryData : IDisposable
    {
        public IDatabase Db;
        private readonly Redlock.CSharp.Redlock _redlock;
        readonly Lock _locker;
        private string _key;
        internal MemoryData(string key, IDatabase db, Redlock.CSharp.Redlock redlock)
        {
            _key = key;
            Db = db;
            _redlock = redlock;

            while (!CheckLock(key, out _locker))
            {
                // blocking
            }
        }

        public string GetLockToken(string key)
        {
            return "LOCKOLOKCO_" + key;
        }

        public bool CheckLock(string key, out Lock locker)
        {
            var lockKey = GetLockToken(key);
            return _redlock.Lock(lockKey, new TimeSpan(0, 0, 10), out locker);
        }

        public void Dispose()
        {
            _redlock.Unlock(_locker);
        }

        public async Task<bool> SetValue<T>(T value)
        {
            var serializer = MessagePackSerializer.Get<T>();
            var msgBytes = serializer.PackSingleObject(value);
            return await SetBytes(msgBytes);
        }

        //public async Task<bool> SetString(string toBase64String)
        //{
        //    return await SetBytes(Encoding.UTF8.GetBytes(toBase64String));
        //}

        async Task<bool> SetBytes(byte[] value)
        {
            lock (_cache)
            {
                _cache.Remove(_key);
            }

            var rValue = await Db.StringSetAsync(_key, value);

            lock (_cache)
            {
                if (_cache.ContainsKey(_key))
                    _cache[_key] = value;
            }
            return rValue;
        }
        public Dictionary<string, RedisValue> _cache = new Dictionary<string, RedisValue>();

        public async Task<T> GetValue<T>()
        {
            var value = await GetBytes();

            var serializer = MessagePackSerializer.Get<T>();
            var getMsg = serializer.UnpackSingleObject(value);

            return getMsg;
        }

        //public async Task<string> GetString()
        //{
        //    var bytes = await GetBytes();
        //    return Encoding.UTF8.GetString(bytes);
        //}

        async Task<byte[]> GetBytes()
        {
            RedisValue rValue;
            lock (_cache)
            {
                if (_cache.TryGetValue(_key, out rValue))
                {
                    return rValue;
                }
            }

            rValue = await Db.StringGetAsync(_key);

            lock (_cache)
            {
                _cache[_key] = rValue;
            }

            return (byte[])rValue;
        }
    }

    public class MemoryDataModule : IDisposable
    {
        private readonly ConnectionMultiplexer _redis;
        public IDatabase Db;
        private readonly Redlock.CSharp.Redlock _redlock;
        public MemoryDataModule()
        {
            _redis = ConnectionMultiplexer.Connect("127.0.0.1");
            _redlock = new Redlock.CSharp.Redlock(_redis);
            Db = _redis.GetDatabase();
        }

        public MemoryData GetData(string key)
        {
            return new MemoryData(key, Db, _redlock);
        }

        public void Dispose()
        {
            _redis.Dispose();
        }
    }
}
