using System;
using System.Collections.Generic;
using System.Linq;

namespace LodTransitions.Rendering.Lods
{
    public class DisposableLruCache<TKey, TVal>
        where TKey : notnull
        where TVal : IDisposable
    {
        private Dictionary<TKey, CacheEntry> cache = new Dictionary<TKey, CacheEntry>();

        public int maxCapacity;

        public DisposableLruCache(int maxCapacity)
        {
            if (maxCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCapacity));
            }
            this.maxCapacity = maxCapacity;
        }

        public TVal GetOrCreate(TKey key, Func<TVal> creator)
        {
            long time = DateTime.Now.Ticks;
            if (this.cache.TryGetValue(key, out CacheEntry? cacheEntry))
            {
                cacheEntry.LastUsedTime = time;
                return cacheEntry.Value;
            }

            while (this.cache.Count >= this.maxCapacity)
            {
                var lruEntry = this.cache.MinBy(v => v.Value.LastUsedTime);
                this.cache.Remove(lruEntry.Key);
                lruEntry.Value.Value.Dispose();
            }

            TVal newValue = creator();
            this.cache[key] = new CacheEntry(time, newValue);
            return newValue;
        }

        private class CacheEntry
        {
            public long LastUsedTime;
            public TVal Value;

            public CacheEntry(long lastUsedTime, TVal value)
            {
                this.LastUsedTime = lastUsedTime;
                this.Value = value;
            }
        }
    }
}
