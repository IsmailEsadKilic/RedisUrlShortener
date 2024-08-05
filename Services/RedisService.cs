using StackExchange.Redis;
using System;
using System.Text.Json;

namespace Redurl.Services
{

    public class RedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly EFService _efService;	

        public RedisService(string connectionString, EFService efService)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();
            _database.Execute("FLUSHDB");
            _efService = efService;
        }

        public void Set(string key, string value)
        {

            _database.StringSet(key, value);
            _database.StringSet(value, key);
            
        }

        public string? Get(string key)
        {
            if (key == null)
            {
                return null;
            }
            return _database.StringGet(key);
        }

        public async Task<string> Shorten(string url)
        {
            // check if url is already shortened
            var key = await GetKey(url);
            if (Get(key) != null)
            {
                return key;
            }

            // generate a new key
            var newKey = Guid.NewGuid().ToString().Substring(0, 8);
            while (Get(newKey) != null)
            {
                newKey = Guid.NewGuid().ToString().Substring(0, 8);
            }

            // save the new key
            Set(newKey, url);

            // save the new key to ef
            await _efService.SaveUrl(newKey, url);

            return newKey;
        }

        private async Task<string?> GetKey(string url)
        {
            // first check redis

            var keyR = Get(url);
            if (keyR != null)
            {
                return keyR;
            }

            //check db for key
    	    var keyEF = await _efService.GetKey(url);       
            return keyEF;         
        }

        public async Task<string?> GetUrl(string key)
        {
            var url = Get(key);
            if (url == null)
            {
                var efResult = await _efService.GetUrl(key);
                if (efResult != null)
                {
                    Set(key, efResult);
                    return efResult;
                }
            }
            return url;
        }

        public async Task<Object> Ls()
        {
            var EFkeys = await _efService.GetKeysValues();
            var RedisKeys = new Dictionary<string, string>();
            foreach (var key in _redis.GetServer(_redis.GetEndPoints()[0]).Keys())
            {
                RedisKeys.Add(key.ToString(), Get(key.ToString()));
            }
            return new { EFkeys, RedisKeys };
        }

        // public void SetObject<T>(string key, T value)
        // {
        //     var json = JsonSerializer.Serialize(value);
        //     _database.StringSet(key, json);
        // }

        // public T GetObject<T>(string key)
        // {
        //     var json = _database.StringGet(key);
        //     return json.HasValue ? JsonSerializer.Deserialize<T>(json) : default;
        // }
    }
}


        // public long Increment(string key)
        // {
        //     return _database.StringIncrement(key);
        // }

        // public long Decrement(string key)
        // {
        //     return _database.StringDecrement(key);
        // }


        // public void Publish(string channel, string message)
        // {
        //     var publisher = _redis.GetSubscriber();
        //     publisher.Publish(channel, message);
        // }

        // public ISubscriber GetSubscriber()
        // {
        //     return _redis.GetSubscriber();
        // }