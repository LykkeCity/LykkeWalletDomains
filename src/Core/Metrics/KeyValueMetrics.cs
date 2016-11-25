using System.Collections.Generic;
using System.Linq;

namespace Core.Metrics
{
    public class KeyValueMetrics
    {
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        public void Put(params KeyValuePair<string, string>[] data)
        {
            lock (_cache)
            {
                foreach (var keyValuePair in data.Where(itm => !string.IsNullOrEmpty(itm.Key)))
                {
                    if (!_cache.ContainsKey(keyValuePair.Value))
                        _cache[keyValuePair.Key] = keyValuePair.Value;
                    else
                        _cache.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        public KeyValuePair<string, string>[] GetData()
        {
            lock (_cache)
            {
                return _cache.ToArray();
            }
        }

    }
}
