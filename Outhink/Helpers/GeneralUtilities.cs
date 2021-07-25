using Microsoft.Extensions.Caching.Memory;
using Outhink.Db.Enums;
using System;
using System.Collections.Generic;

namespace Outhink.Helpers
{
    public static class GeneralUtilities
    {
        public static void CleanCache(IMemoryCache _cache, Dictionary<string, int> coins)
        {
            foreach (int i in Enum.GetValues(typeof(CoinType)))
            {
                var key = Enum.GetName(typeof(CoinType), i);
                var valueCached = _cache.TryGetValue(key, out int value);
                if (valueCached)
                {
                    coins.Add(key, value);
                    _cache.Remove(key);
                }
            }
        }

        public static int SumCachedCoins(Dictionary<string, int> coins)
        {
            int totalValue = 0;
            foreach (var item in coins)
            {
                var enumType = (CoinType)Enum.Parse(typeof(CoinType), item.Key);
                switch (enumType)
                {
                    case CoinType.TenCent:
                        totalValue += (10 * item.Value);
                        break;
                    case CoinType.TwentyCent:
                        totalValue += (20 * item.Value);
                        break;
                    case CoinType.FiftyCent:
                        totalValue += (50 * item.Value);
                        break;
                    case CoinType.OneEuro:
                        totalValue += (100 * item.Value);
                        break;
                    default:
                        break;
                }
            }
            return totalValue;
        }
    }
}
