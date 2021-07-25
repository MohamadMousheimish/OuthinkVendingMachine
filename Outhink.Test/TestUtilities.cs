using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using NSubstitute;
using Outhink.AutoMapper;
using Outhink.Db.Context;
using Outhink.Db.Enums;
using Outhink.Db.Models;
using Outhink.RequestModels.CommandRequestModels;
using Outhink.RequestModels.QueryRequestModels;
using Outhink.ResponseModels.QueryResponseModels;
using System;
using System.Threading.Tasks;

namespace Outhink.Test
{
    public static class TestUtilities
    {
        internal static IMapper CreateMapper()
        {
            //Auto mapper configuration
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ItemProfile());
                cfg.AddProfile(new CoinProfile());
            });
            var mapper = mockMapper.CreateMapper();
            return mapper;
        }

        public static Coin CreateCoin(int quantity, CoinType coinType)
        {
            return new Coin
            {
                Quantity = quantity,
                Type = coinType
            };
        }

        public static Item CreateItem(string name, double price, int quantity)
        {
            return new Item
            {
                Name = name,
                Price = price,
                Quantity = quantity
            };
        }

        public static InsertCoinsRequestModel CreateInsertCoinsRequestModel(CoinType coinType)
        {
            return new InsertCoinsRequestModel
            {
                Type = coinType
            };
        }

        public static GetCoinsRequestModel CreateCoinsRequestModel()
        {
            return new GetCoinsRequestModel();
        }

        public static GetItemsRequestModel CreateItemsRequestModel(int skip = 0, int take = 10)
        {
            return new GetItemsRequestModel
            {
                Skip = skip,
                Take = take
            };
        }

        public static GetCoinsResponseModel CreateCoinsResponseModel(int quantity, string coinType)
        {
            return new GetCoinsResponseModel
            {
                Quantity = quantity,
                Type = coinType
            };
        }

        public static GetItemsResponseModel CreateItemsResponseModel(int id, string name, double price)
        {
            return new GetItemsResponseModel
            {
                Id = id,
                Name = name,
                Price = price
            };
        }

        public static MakeOrderRequestModel CreateMakeOrderRequestModel(int itemId)
        {
            return new MakeOrderRequestModel
            {
                ItemId = itemId
            };
        }

        public async static Task CleanData(OuthinkContext context)
        {
            foreach (var entity in context.Coins)
            {
                context.Coins.Remove(entity);
            }
            foreach (var entity in context.Items)
            {
                context.Items.Remove(entity);
            }
            await context.SaveChangesAsync();
        }

        public static void CleanCache(IMemoryCache cache)
        {
            foreach (int i in Enum.GetValues(typeof(CoinType)))
            {
                var key = Enum.GetName(typeof(CoinType), i);
                var valueCached = cache.TryGetValue(key, out int _);
                if (valueCached)
                {
                    cache.Remove(key);
                }
            }
        }

        public static MemoryCache CreateTestingMemoryCache()
        {
            var cacheOptions = Substitute.For<IOptions<MemoryCacheOptions>>();
            cacheOptions.Value.Returns(new MemoryCacheOptions() { Clock = new SystemClock() });
            var cache = new MemoryCache(cacheOptions);
            return cache;
        }
    }
}
