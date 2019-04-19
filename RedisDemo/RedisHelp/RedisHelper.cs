using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisDemo.RedisHelp
{
    public class RedisHelper
    {
        private readonly ConnectionMultiplexer _conn;
        private readonly IDatabase _database;

        public string CustomKey { get; set; }

        #region Construct functions

        public RedisHelper(int dbNum = 0)
                : this(dbNum, null)
        {

        }

        public RedisHelper(int dbNum, string readWriteHosts)
        {
            _conn = string.IsNullOrWhiteSpace(readWriteHosts) ? RedisConnectionHelp.Instance : RedisConnectionHelp.GetConnectionMultiplexer(readWriteHosts);
            _database = _conn.GetDatabase(dbNum);
            CustomKey = RedisConnectionHelp.SysCustomKey;
        }

        #region Helper methods

        private string AddSysCustomKey(string oldKey)
        {
            return CustomKey + oldKey;
        }

        private T Do<T>(Func<IDatabase, T> func)
        {
            return func(_database);
        }

        private string ConvertJson<T>(T value)
        {
            string result = value is string ? value.ToString() : JsonConvert.SerializeObject(value);
            return result;
        }

        private T ConvertObj<T>(RedisValue value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        private List<T> ConvetList<T>(RedisValue[] values)
        {
            List<T> result = new List<T>();
            foreach (var item in values)
            {
                var model = ConvertObj<T>(item);
                result.Add(model);
            }
            return result;
        }

        private RedisKey[] ConvertRedisKeys(List<string> redisKeys)
        {
            return redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();
        }

        #endregion Helper methods

        #endregion Construct functions

        #region String

        #region Sync methods

        public bool StringSet(string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringSet(key, value, expiry));
        }

        public bool StringSet(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            IEnumerable<KeyValuePair<RedisKey, RedisValue>> newkeyValues = keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value));
            return Do(db => db.StringSet(newkeyValues.ToArray()));
        }

        public bool StringSet<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            string json = ConvertJson(obj);
            return Do(db => db.StringSet(key, json, expiry));
        }

        public string StringGet(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringGet(key));
        }

        public RedisValue[] StringGet(List<string> listKey)
        {
            List<string> newKeys = listKey.Select(AddSysCustomKey).ToList();
            return Do(db => db.StringGet(ConvertRedisKeys(newKeys)));
        }

        public T StringGet<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => ConvertObj<T>(db.StringGet(key)));
        }

        public double StringIncrement(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringIncrement(key, val));
        }

        public double StringDecrement(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringDecrement(key, val));
        }

        #endregion Sync methods

        #region Async methods

        public async Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.StringSetAsync(key, value, expiry));
        }

        public async Task<bool> StringSetAsync(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            IEnumerable<KeyValuePair<RedisKey, RedisValue>> newkeyValues = keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value));
            return await Do(db => db.StringSetAsync(newkeyValues.ToArray()));
        }

        public async Task<bool> StringSetAsync<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            string json = ConvertJson(obj);
            return await Do(db => db.StringSetAsync(key, json, expiry));
        }

        public async Task<string> StringGetAsync(string key)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.StringGetAsync(key));
        }

        public async Task<RedisValue[]> StringGetAsync(List<string> listKey)
        {
            List<string> newKeys = listKey.Select(AddSysCustomKey).ToList();
            return await Do(db => db.StringGetAsync(ConvertRedisKeys(newKeys)));
        }

        public async Task<T> StringGetAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            string result = await Do(db => db.StringGetAsync(key));
            return ConvertObj<T>(result);
        }

        public async Task<double> StringIncrementAsync(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.StringIncrementAsync(key, val));
        }

        public async Task<double> StringDecrementAsync(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.StringDecrementAsync(key, val));
        }

        #endregion Async methods

        #endregion String

        #region List

        #region Sync methods

        public long ListRemove<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.ListRemove(key, ConvertJson(value)));
        }

        public List<T> ListRange<T>(string key, long start = 0, long stop = -1)
        {
            key = AddSysCustomKey(key);
            return Do(redis =>
            {
                var values = redis.ListRange(key, start, stop);
                return ConvetList<T>(values);
            });
        }

        public long ListRightPush<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.ListRightPush(key, ConvertJson(value)));
        }

        public T ListRightPop<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var value = db.ListRightPop(key);
                return ConvertObj<T>(value);
            });
        }

        public long ListLeftPush<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.ListLeftPush(key, ConvertJson(value)));
        }

        public T ListLeftPop<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var value = db.ListLeftPop(key);
                return ConvertObj<T>(value);
            });
        }

        public long ListLength(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.ListLength(key));
        }

        #endregion Sync methods

        #region Async methods

        public async Task<long> ListRemoveAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.ListRemoveAsync(key, ConvertJson(value)));
        }

        public async Task<List<T>> ListRangeAsync<T>(string key, long start = 0, long stop = -1)
        {
            key = AddSysCustomKey(key);
            RedisValue[] values = await Do(redis => redis.ListRangeAsync(key, start, stop));
            return ConvetList<T>(values);
        }

        public async Task<long> ListRightPushAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.ListRightPushAsync(key, ConvertJson(value)));
        }

        public async Task<T> ListRightPopAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            var value = await Do(db => db.ListRightPopAsync(key));
            return ConvertObj<T>(value);
        }

        public async Task<long> ListLeftPushAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.ListLeftPushAsync(key, ConvertJson(value)));
        }

        public async Task<T> ListLeftPopAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            var value = await Do(db => db.ListLeftPopAsync(key));
            return ConvertObj<T>(value);
        }

        public async Task<long> ListLengthAsync(string key)
        {
            key = AddSysCustomKey(key);
            return await Do(redis => redis.ListLengthAsync(key));
        }

        #endregion Async methods

        #endregion List

        #region Hash

        #region Sync methods

        public bool HashExists(string key, string hashKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashExists(key, hashKey));
        }

        public bool HashSet<T>(string key, string hashKey, T t)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                string json = ConvertJson(t);
                return db.HashSet(key, hashKey, json);
            });
        }

        public bool HashDelete(string key, string hashKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDelete(key, hashKey));
        }

        public long HashDelete(string key, RedisValue[] hashKeys)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDelete(key, hashKeys));
        }

        public T HashGet<T>(string key, string hashKey)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                string value = db.HashGet(key, hashKey);
                return ConvertObj<T>(value);
            });
        }

        public double HashIncrement(string key, string hashKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashIncrement(key, hashKey, val));
        }

        public double HashDecrement(string key, string hashKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDecrement(key, hashKey, val));
        }

        public List<T> HashKeys<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                RedisValue[] values = db.HashKeys(key);
                return ConvetList<T>(values);
            });
        }

        #endregion Sync methods

        #region Async methods

        public async Task<bool> HashExistsAsync(string key, string hashKey)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.HashExistsAsync(key, hashKey));
        }

        public async Task<bool> HashSetAsync<T>(string key, string hashKey, T t)
        {
            key = AddSysCustomKey(key);
            return await Do(db =>
            {
                string json = ConvertJson(t);
                return db.HashSetAsync(key, hashKey, json);
            });
        }

        public async Task<bool> HashDeleteAsync(string key, string hashKey)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.HashDeleteAsync(key, hashKey));
        }

        public async Task<long> HashDeleteAsync(string key, RedisValue[] hashKeys)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.HashDeleteAsync(key, hashKeys));
        }

        public async Task<T> HashGeAsync<T>(string key, string hashKey)
        {
            key = AddSysCustomKey(key);
            string value = await Do(db => db.HashGetAsync(key, hashKey));
            return ConvertObj<T>(value);
        }

        public async Task<double> HashIncrementAsync(string key, string hashKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.HashIncrementAsync(key, hashKey, val));
        }

        public async Task<double> HashDecrementAsync(string key, string hashKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return await Do(db => db.HashDecrementAsync(key, hashKey, val));
        }

        public async Task<List<T>> HashKeysAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            RedisValue[] values = await Do(db => db.HashKeysAsync(key));
            return ConvetList<T>(values);
        }

        #endregion Async methods

        #endregion Hash

        #region SortedSet

        #region Sync methods

        public bool SortedSetAdd<T>(string key, T value, double score)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.SortedSetAdd(key, ConvertJson<T>(value), score));
        }

        public bool SortedSetRemove<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.SortedSetRemove(key, ConvertJson(value)));
        }

        public List<T> SortedSetRangeByRank<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis =>
            {
                var values = redis.SortedSetRangeByRank(key);
                return ConvetList<T>(values);
            });
        }

        public long SortedSetLength(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.SortedSetLength(key));
        }

        #endregion Sync methods

        #region Async methods

        public async Task<bool> SortedSetAddAsync<T>(string key, T value, double score)
        {
            key = AddSysCustomKey(key);
            return await Do(redis => redis.SortedSetAddAsync(key, ConvertJson<T>(value), score));
        }

        public async Task<bool> SortedSetRemoveAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return await Do(redis => redis.SortedSetRemoveAsync(key, ConvertJson(value)));
        }

        public async Task<List<T>> SortedSetRangeByRankAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            var values = await Do(redis => redis.SortedSetRangeByRankAsync(key));
            return ConvetList<T>(values);
        }

        public async Task<long> SortedSetLengthAsync(string key)
        {
            key = AddSysCustomKey(key);
            return await Do(redis => redis.SortedSetLengthAsync(key));
        }

        #endregion Async methods

        #endregion SortedSet

        #region Key

        public bool KeyDelete(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyDelete(key));
        }

        public long KeyDelete(List<string> keys)
        {
            List<string> newKeys = keys.Select(AddSysCustomKey).ToList();
            return Do(db => db.KeyDelete(ConvertRedisKeys(newKeys)));
        }

        public bool KeyExists(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyExists(key));
        }

        public bool KeyRename(string key, string newKey)
        {
            key = AddSysCustomKey(key);
            newKey = AddSysCustomKey(newKey);
            return Do(db => db.KeyRename(key, newKey));
        }

        public bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyExpire(key, expiry));
        }

        #endregion Key

        #region Subscribe

        #region Sync methods

        public void Subscribe(string subChannel, Action<RedisChannel, RedisValue> handler = null)
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.Subscribe(subChannel, (channel, message) =>
            {
                if (handler == null)
                {
                    Console.WriteLine(subChannel + " receive message：" + message);
                }
                else
                {
                    handler(channel, message);
                }
            });
        }

        public long Publish<T>(string channel, T message)
        {
            ISubscriber sub = _conn.GetSubscriber();
            return sub.Publish(channel, ConvertJson(message));
        }

        public void Unsubscribe(string channel)
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.Unsubscribe(channel);
        }

        public void UnsubscribeAll()
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.UnsubscribeAll();
        }

        #endregion Sync methods

        #region Async methods

        public async Task SubscribeAsync(string subChannel, Action<RedisChannel, RedisValue> handler = null)
        {
            ISubscriber sub = _conn.GetSubscriber();
            await sub.SubscribeAsync(subChannel, (channel, message) =>
            {
                if (handler == null)
                {
                    Console.WriteLine(subChannel + " receive message：" + message);
                }
                else
                {
                    handler(channel, message);
                }
            });
        }

        public async Task<long> PublishAsync<T>(string channel, T message)
        {
            ISubscriber sub = _conn.GetSubscriber();
            return await sub.PublishAsync(channel, ConvertJson(message));
        }

        public async Task UnsubscribeAsync(string channel)
        {
            ISubscriber sub = _conn.GetSubscriber();
            await sub.UnsubscribeAsync(channel);
        }

        public async Task UnsubscribeAllAsync()
        {
            ISubscriber sub = _conn.GetSubscriber();
            await sub.UnsubscribeAllAsync();
        }

        #endregion Async methods

        #endregion Subscribe

        #region Other

        public ITransaction CreateTransaction()
        {
            return GetDatabase().CreateTransaction();
        }

        public IDatabase GetDatabase()
        {
            return _database;
        }

        public IServer GetServer(string hostAndPort)
        {
            return _conn.GetServer(hostAndPort);
        }

        public void SetSysCustomKey(string customKey)
        {
            CustomKey = customKey;
        }

        public int GetHashSlot(string key)
        {
            return _conn.GetHashSlot(key);
        }

        #endregion Other
    }
}
