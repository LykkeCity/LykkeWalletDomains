using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Metrics
{
    public class MetricData
    {
        private readonly Dictionary<string, List<KeyValuePair<DateTime, KeyValuePair<string,string>[]>>> _cache =
            new Dictionary<string, List<KeyValuePair<DateTime, KeyValuePair<string, string>[]>>>();


        private readonly Dictionary<string, string> _dataHashes = new Dictionary<string, string>();

        public void Push(string collectionName, params KeyValuePair<string, string>[] events)
        {

            if (string.IsNullOrEmpty(collectionName))
                return;

            lock (_cache)
            {
                if (!_cache.ContainsKey(collectionName))
                    _cache.Add(collectionName, new List<KeyValuePair<DateTime, KeyValuePair<string, string>[]>>());

                var collection = _cache[collectionName];

                var filterEmptyKeys = events.Where(itm => !string.IsNullOrEmpty(itm.Key)).ToArray();

                collection.Insert(0, new KeyValuePair<DateTime, KeyValuePair<string, string>[]>(DateTime.UtcNow, filterEmptyKeys));

                while (collection.Count > MaxRecords)
                    collection.RemoveAt(collection.Count-1);


                if (!_dataHashes.ContainsKey(collectionName))
                    _dataHashes.Add(collectionName, Guid.NewGuid().ToString("N"));
                else
                    _dataHashes[collectionName] = Guid.NewGuid().ToString("N");
            }

        }


        private static readonly SortedDictionary<DateTime, KeyValuePair<string, string>[]> NullData 
            = new SortedDictionary<DateTime, KeyValuePair<string, string>[]>();

        private const int MaxRecords = 50;

        public SortedDictionary<DateTime, KeyValuePair<string, string>[]> GetData(string collectionName)
        {
            lock (_cache)
            {
                if (!_cache.ContainsKey(collectionName))
                    return NullData;

                var result = new SortedDictionary<DateTime, KeyValuePair<string,string>[]>();

                foreach (var keyValuePair in _cache[collectionName])
                {
                    result.Add(keyValuePair.Key, keyValuePair.Value);
                }


                return result;
            }
        }


        public string GetHash(string collectionName)
        {
            lock (_cache)
            {
                return _dataHashes.ContainsKey(collectionName) ? _dataHashes[collectionName] : Guid.NewGuid().ToString("N");
            }
        }


        public IEnumerable<string> GetCollections()
        {
            lock (_cache)
            {
                return  _cache.Keys.ToArray();
            }
        }

    }

}
