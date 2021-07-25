using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Outhink.Db.Enums;
using Outhink.Db.Models;
using Outhink.Db.Repositories;
using Outhink.Helpers;
using Outhink.RequestModels.CommandRequestModels;
using Outhink.ResponseModels.CommandResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Outhink.Handlers.CommandHandlers
{
    public class MakeOrderCommandHandler : IRequestHandler<MakeOrderRequestModel, MakeOrderResponseModel>
    {
        private readonly IBaseRepository<Item> _itemRepository;
        private readonly IBaseRepository<Coin> _coinRepository;
        private readonly IMemoryCache _cache;

        public MakeOrderCommandHandler(IBaseRepository<Item> itemRepository, IBaseRepository<Coin> coinRepository,
            IMemoryCache cache)
        {
            _itemRepository = itemRepository;
            _coinRepository = coinRepository;
            _cache = cache;
        }

        public async Task<MakeOrderResponseModel> Handle(MakeOrderRequestModel request, CancellationToken cancellationToken)
        {
            MakeOrderResponseModel responseModel = new();
            var soldItem = await _itemRepository.GetByIdAsync(request.ItemId);
            //Getting the inserted coins
            var coins = GetCachedCoins();

            if (soldItem != null)
            {
                //Adding the inserted coins
                var totalPayedCoins = GeneralUtilities.SumCachedCoins(coins);
                if (totalPayedCoins == 0)
                {
                    responseModel.Succeeded = false;
                    responseModel.Note = "Please insert some coins first!";
                    return responseModel;
                }

                //Checking if any of the item still remain
                if (soldItem.Quantity == 0)
                {
                    responseModel.Succeeded = false;
                    responseModel.Note = "Item is sold out";
                    responseModel.ReturnedCoins = coins;
                    return responseModel;
                }

                var vendingPrice = (int)(soldItem.Price * 100);

                //Checking if sum of inserted coin is enough to buy the item
                if (totalPayedCoins < vendingPrice)
                {
                    responseModel.Succeeded = false;
                    responseModel.Note = "Insufficient amount";
                    responseModel.ReturnedCoins = coins;
                    return responseModel;
                }

                var dbCoins = await _coinRepository.ListAllAsync();

                //Adding the new coins to the vending machine
                await AddCoinsToVendingMachine(coins, dbCoins);

                //Removing one protion from the sold item
                soldItem.Quantity--;
                await _itemRepository.UpdateAsync(soldItem);

                //Getting the rest to return coins
                var restValue = totalPayedCoins - vendingPrice;

                //Check if the vending machine need to return coins
                if (restValue > 0)
                {
                    int eurosReturned = restValue / 100;
                    int centsReturned = restValue % 100;
                    int fiftiesReturned = centsReturned / 50;
                    int fiftiesRemaining = centsReturned % 50;
                    int twentiesReturned = fiftiesRemaining / 20;
                    int twentiesRemaning = fiftiesRemaining % 20;
                    int tenReturned = twentiesRemaning / 10;

                    if (eurosReturned > 0)
                    {
                        var dbEuroCoins = dbCoins.FirstOrDefault(c => c.Type == CoinType.OneEuro);
                        if (dbEuroCoins.Quantity >= eurosReturned)
                        {
                            responseModel.ReturnedCoins.Add(CoinType.OneEuro.ToString(), eurosReturned);
                            dbEuroCoins.Quantity -= eurosReturned;
                        }
                        else
                        {
                            var leftEuros = eurosReturned - dbEuroCoins.Quantity;
                            if (dbEuroCoins.Quantity > 0)
                            {
                                responseModel.ReturnedCoins.Add(CoinType.OneEuro.ToString(), eurosReturned - leftEuros);
                                dbEuroCoins.Quantity = 0;
                            }
                            fiftiesReturned += (leftEuros * 2);
                        }
                        await _coinRepository.UpdateAsync(dbEuroCoins);
                    }

                    if (fiftiesReturned > 0)
                    {
                        var dbfiftyCoins = dbCoins.FirstOrDefault(c => c.Type == CoinType.FiftyCent);
                        if (dbfiftyCoins.Quantity >= fiftiesReturned)
                        {
                            responseModel.ReturnedCoins.Add(CoinType.FiftyCent.ToString(), fiftiesReturned);
                            dbfiftyCoins.Quantity -= fiftiesReturned;
                        }
                        else
                        {
                            var leftFifties = fiftiesReturned - dbfiftyCoins.Quantity;
                            if (dbfiftyCoins.Quantity > 0)
                            {
                                responseModel.ReturnedCoins.Add(CoinType.FiftyCent.ToString(), fiftiesReturned - leftFifties);
                                dbfiftyCoins.Quantity = 0;
                            }
                            //Checking if left fifty cent are devided by 2
                            //Because if it is, then we can send one twenty cent instead of two 10 cent (for each two fifties)
                            if (leftFifties % 2 == 0)
                            {
                                twentiesReturned += (leftFifties * 2) + 1;
                            }
                            else
                            {
                                twentiesReturned += (leftFifties * 2);
                                tenReturned += leftFifties;
                            }
                        }
                        await _coinRepository.UpdateAsync(dbfiftyCoins);
                    }

                    if (twentiesReturned > 0)
                    {
                        var dbTwentyCoins = dbCoins.FirstOrDefault(c => c.Type == CoinType.TwentyCent);
                        if (dbTwentyCoins.Quantity >= twentiesReturned)
                        {
                            responseModel.ReturnedCoins.Add(CoinType.TwentyCent.ToString(), twentiesReturned);
                            dbTwentyCoins.Quantity -= twentiesReturned;
                        }
                        else
                        {
                            var leftTwenties = twentiesReturned - dbTwentyCoins.Quantity;
                            if (dbTwentyCoins.Quantity > 0)
                            {
                                responseModel.ReturnedCoins.Add(CoinType.TwentyCent.ToString(), twentiesReturned - leftTwenties);
                                dbTwentyCoins.Quantity = 0;
                            }
                            tenReturned += (leftTwenties * 2);
                        }
                        await _coinRepository.UpdateAsync(dbTwentyCoins);
                    }

                    if (tenReturned > 0)
                    {
                        var dbTenCoins = dbCoins.FirstOrDefault(c => c.Type == CoinType.TenCent);
                        if (dbTenCoins.Quantity >= tenReturned)
                        {
                            responseModel.ReturnedCoins.Add(CoinType.TenCent.ToString(), tenReturned);
                            dbTenCoins.Quantity -= tenReturned;
                        }
                        else
                        {
                            //Don't know what would happen if no ten cents exist in the vending machine
                        }
                        await _coinRepository.UpdateAsync(dbTenCoins);
                    }
                }
            }
            else
            {
                responseModel.Succeeded = false;
                responseModel.Note = "Item does not exist";
                responseModel.ReturnedCoins = coins;
            }
            return responseModel;
        }

        private async Task AddCoinsToVendingMachine(Dictionary<string, int> coins, IEnumerable<Coin> dbCoins)
        {
            foreach (var item in coins)
            {
                var enumType = (CoinType)Enum.Parse(typeof(CoinType), item.Key);
                var updatedRecord = dbCoins.FirstOrDefault(c => c.Type == enumType);
                updatedRecord.Quantity += item.Value;
                await _coinRepository.UpdateAsync(updatedRecord);
            }
        }

        private Dictionary<string, int> GetCachedCoins()
        {
            Dictionary<string, int> coins = new();
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
            return coins;
        }
    }
}
