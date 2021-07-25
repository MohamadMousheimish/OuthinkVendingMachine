using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Outhink.Db.Context;
using Outhink.Db.Enums;
using Outhink.Db.Models;
using Outhink.Db.Repositories;
using Outhink.Handlers.CommandHandlers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Outhink.Test.Handlers.CommandHandlers
{
    public class MakeOrderCommandHandlerTest
    {
        private readonly IMemoryCache _cache;
        private readonly IBaseRepository<Coin> _coinrepository;
        private readonly IBaseRepository<Item> _itemRepository;
        private readonly MakeOrderCommandHandler _handler;
        private readonly OuthinkContext _context;
        public MakeOrderCommandHandlerTest()
        {
            var options = new DbContextOptionsBuilder<OuthinkContext>()
                .UseInMemoryDatabase(databaseName: "MakeOrderCommandHandlerTest")
                .Options;
            _context = new OuthinkContext(options);
            _cache = TestUtilities.CreateTestingMemoryCache();
            _coinrepository = new BaseRepository<Coin>(_context);
            _itemRepository = new BaseRepository<Item>(_context);
            _handler = new MakeOrderCommandHandler(_itemRepository, _coinrepository, _cache);
        }

        /// <summary>
        /// Unit test which will call the buy order handler
        /// This unit test will try to buy a sold out item
        /// </summary>
        [Fact]
        public async Task BuyOrder_SoldOut_Test()
        {
            var firstInsertedCoin = CoinType.OneEuro;
            var secondInsertedCoin = CoinType.FiftyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 0);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);

            try
            {
                #region Arrange

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(firstInsertedCoin.ToString(), 1);
                _cache.Set(secondInsertedCoin.ToString(), 2);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.False(result.Succeeded);
                Assert.Equal("Item is sold out", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Equal(2, result.ReturnedCoins.Count);
                Assert.Contains(result.ReturnedCoins, c => c.Key == firstInsertedCoin.ToString() && c.Value == 1);
                Assert.Contains(result.ReturnedCoins, c => c.Key == secondInsertedCoin.ToString() && c.Value == 2);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        /// <summary>
        /// Unit test which will call the buy order handler
        /// This unit test will try to buy an item with less than its price
        /// </summary>
        [Fact]
        public async Task BuyOrder_LessThanAmount_Test()
        {
            var secondInsertedCoin = CoinType.FiftyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);

            try
            {
                #region Arrange

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(secondInsertedCoin.ToString(), 1);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.False(result.Succeeded);
                Assert.Equal("Insufficient amount", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Single(result.ReturnedCoins);
                Assert.Contains(result.ReturnedCoins, c => c.Key == secondInsertedCoin.ToString() && c.Value == 1);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        /// <summary>
        /// Unit test which will call the buy order handler
        /// This unit test will try to buy an item without inserting coins
        /// </summary>
        [Fact]
        public async Task BuyOrder_WithoutInsertingCoin_Test()
        {
            try
            {
                var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
                var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);

                #region Arrange

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.False(result.Succeeded);
                Assert.Equal("Please insert some coins first!", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Empty(result.ReturnedCoins);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        /// <summary>
        /// Unit test which will call the buy order handler
        /// This unit test will try to buy an item that don't exist
        /// </summary>
        [Fact]
        public async Task BuyOrder_ItemDontExist_Test()
        {

            var firstInsertedCoin = CoinType.OneEuro;
            try
            {
                #region Arrange

                var requestModel = TestUtilities.CreateMakeOrderRequestModel(-1);
                _cache.Set(firstInsertedCoin.ToString(), 1);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.False(result.Succeeded);
                Assert.Equal("Item does not exist", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Single(result.ReturnedCoins);
                Assert.Contains(result.ReturnedCoins, c => c.Key == firstInsertedCoin.ToString() && c.Value == 1);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        /// <summary>
        /// Unit test which will call the buy order handler
        /// This unit test will only check the increment and decrement of vending machine coins
        /// </summary>
        [Fact]
        public async Task BuyOrder_CheckVendingMachineCoins_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(secondItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 1);
                _cache.Set(fiftyCentCoin.ToString(), 2);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Single(result.ReturnedCoins);
                Assert.Contains(result.ReturnedCoins, c => c.Key == twentyCentCoin.ToString() && c.Value == 1);

                //One euro should be added to vending machine
                Assert.Equal(6, dbOneEuro.Quantity);

                //Two 50 cent should be added to vending machine
                Assert.Equal(7, dbFiftyCent.Quantity);

                //One 20 cent should be removed from vending machine
                Assert.Equal(4, dbTwentyCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        /// <summary>
        /// Unit test which will call the buy order handler
        /// This unit test will only check portions number after buying an item
        /// </summary>
        [Fact]
        public async Task BuyOrder_CheckVendingMachinePortions_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(secondItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 1);
                _cache.Set(fiftyCentCoin.ToString(), 2);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.Equal(1, secondItem.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        /// <summary>
        /// Unit test which will call the buy order handler
        /// This unit test will buy an item with the exact same price inserted
        /// Will check if no coins are returned and portions are removed from machine
        /// </summary>
        [Fact]
        public async Task BuyOrder_ExactPrice_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 1);
                _cache.Set(twentyCentCoin.ToString(), 1);
                _cache.Set(tenCentCoin.ToString(), 1);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Empty(result.ReturnedCoins);
                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(6, dbOneEuro.Quantity);
                Assert.Equal(6, dbTwentyCent.Quantity);
                Assert.Equal(6, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }


        #region Vending Machine Should Return Coins Test

        [Fact]
        public async Task ShouldReturn_OneEuro_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 2);
                _cache.Set(twentyCentCoin.ToString(), 1);
                _cache.Set(tenCentCoin.ToString(), 1);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Single(result.ReturnedCoins);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.OneEuro.ToString() && c.Value == 1);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(6, dbOneEuro.Quantity);
                Assert.Equal(5, dbFiftyCent.Quantity);
                Assert.Equal(6, dbTwentyCent.Quantity);
                Assert.Equal(6, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_FiftyCent_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 1);
                _cache.Set(fiftyCentCoin.ToString(), 1);
                _cache.Set(twentyCentCoin.ToString(), 1);
                _cache.Set(tenCentCoin.ToString(), 1);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Single(result.ReturnedCoins);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.FiftyCent.ToString() && c.Value == 1);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(6, dbOneEuro.Quantity);
                Assert.Equal(5, dbFiftyCent.Quantity);
                Assert.Equal(6, dbTwentyCent.Quantity);
                Assert.Equal(6, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_TwentyCent_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 1);
                _cache.Set(twentyCentCoin.ToString(), 2);
                _cache.Set(tenCentCoin.ToString(), 1);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Single(result.ReturnedCoins);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.TwentyCent.ToString() && c.Value == 1);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(6, dbOneEuro.Quantity);
                Assert.Equal(5, dbFiftyCent.Quantity);
                Assert.Equal(6, dbTwentyCent.Quantity);
                Assert.Equal(6, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_TenCent_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 1);
                _cache.Set(twentyCentCoin.ToString(), 1);
                _cache.Set(tenCentCoin.ToString(), 2);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Single(result.ReturnedCoins);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.TenCent.ToString() && c.Value == 1);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(6, dbOneEuro.Quantity);
                Assert.Equal(5, dbFiftyCent.Quantity);
                Assert.Equal(6, dbTwentyCent.Quantity);
                Assert.Equal(6, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_OneEuro_FiftyCent_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 2);
                _cache.Set(fiftyCentCoin.ToString(), 1);
                _cache.Set(twentyCentCoin.ToString(), 1);
                _cache.Set(tenCentCoin.ToString(), 1);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Equal(2, result.ReturnedCoins.Count);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.OneEuro.ToString() && c.Value == 1);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.FiftyCent.ToString() && c.Value == 1);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(6, dbOneEuro.Quantity);
                Assert.Equal(5, dbFiftyCent.Quantity);
                Assert.Equal(6, dbTwentyCent.Quantity);
                Assert.Equal(6, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_OneEuro_TwentyCent_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 2);
                _cache.Set(twentyCentCoin.ToString(), 2);
                _cache.Set(tenCentCoin.ToString(), 1);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Equal(2, result.ReturnedCoins.Count);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.OneEuro.ToString() && c.Value == 1);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.TwentyCent.ToString() && c.Value == 1);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(6, dbOneEuro.Quantity);
                Assert.Equal(5, dbFiftyCent.Quantity);
                Assert.Equal(6, dbTwentyCent.Quantity);
                Assert.Equal(6, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_OneEuro_TenCent_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 2);
                _cache.Set(twentyCentCoin.ToString(), 1);
                _cache.Set(tenCentCoin.ToString(), 2);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Equal(2, result.ReturnedCoins.Count);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.OneEuro.ToString() && c.Value == 1);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.TenCent.ToString() && c.Value == 1);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(6, dbOneEuro.Quantity);
                Assert.Equal(5, dbFiftyCent.Quantity);
                Assert.Equal(6, dbTwentyCent.Quantity);
                Assert.Equal(6, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_TwoFifties_InsteadOf_OneEuro_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(0, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(5, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(fiftyCentCoin.ToString(), 4);
                _cache.Set(twentyCentCoin.ToString(), 1);
                _cache.Set(tenCentCoin.ToString(), 1);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Single(result.ReturnedCoins);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.FiftyCent.ToString() && c.Value == 2);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(0, dbOneEuro.Quantity);
                Assert.Equal(7, dbFiftyCent.Quantity);
                Assert.Equal(6, dbTwentyCent.Quantity);
                Assert.Equal(6, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_FiveTwenties_InsteadOf_TwoFifty_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(0, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(0, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(twentyCentCoin.ToString(), 10);
                _cache.Set(tenCentCoin.ToString(), 3);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Single(result.ReturnedCoins);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.TwentyCent.ToString() && c.Value == 5);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(0, dbOneEuro.Quantity);
                Assert.Equal(0, dbFiftyCent.Quantity);
                Assert.Equal(10, dbTwentyCent.Quantity);
                Assert.Equal(8, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_TwoTwenties_OneTen_InsteadOf_OneFifty_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(0, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(0, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(twentyCentCoin.ToString(), 9);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Equal(2, result.ReturnedCoins.Count);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.TwentyCent.ToString() && c.Value == 2);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.TenCent.ToString() && c.Value == 1);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(0, dbOneEuro.Quantity);
                Assert.Equal(0, dbFiftyCent.Quantity);
                Assert.Equal(12, dbTwentyCent.Quantity);
                Assert.Equal(4, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_TwoTen_InsteadOf_OneTwenty_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(0, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(0, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(0, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(tenCentCoin.ToString(), 15);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Single(result.ReturnedCoins);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.TenCent.ToString() && c.Value == 2);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(0, dbOneEuro.Quantity);
                Assert.Equal(0, dbFiftyCent.Quantity);
                Assert.Equal(0, dbTwentyCent.Quantity);
                Assert.Equal(18, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        [Fact]
        public async Task ShouldReturn_OneEuro_TwoTwenty_OneTen_InsteadOf_FiftyCent_Test()
        {
            var oneEuroCoin = CoinType.OneEuro;
            var fiftyCentCoin = CoinType.FiftyCent;
            var tenCentCoin = CoinType.TenCent;
            var twentyCentCoin = CoinType.TwentyCent;
            var firstItem = TestUtilities.CreateItem("Juice", 1.3, 2);
            var secondItem = TestUtilities.CreateItem("Espresso", 1.8, 2);
            try
            {
                #region Arrange

                #region Adding Coins to Database

                var dbOneEuro = TestUtilities.CreateCoin(5, oneEuroCoin);
                var dbFiftyCent = TestUtilities.CreateCoin(0, fiftyCentCoin);
                var dbTwentyCent = TestUtilities.CreateCoin(5, twentyCentCoin);
                var dbTenCent = TestUtilities.CreateCoin(5, tenCentCoin);
                var listOfCoins = new List<Coin>
                {
                    dbOneEuro,
                    dbFiftyCent,
                    dbTwentyCent,
                    dbTenCent
                };
                await _coinrepository.AddRangeAsync(listOfCoins);

                #endregion

                await _itemRepository.AddAsync(firstItem);
                await _itemRepository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateMakeOrderRequestModel(firstItem.Id);
                _cache.Set(oneEuroCoin.ToString(), 2);
                _cache.Set(twentyCentCoin.ToString(), 3);
                _cache.Set(tenCentCoin.ToString(), 2);

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.True(result.Succeeded);
                Assert.Equal("Thank you", result.Note);
                Assert.NotNull(result.ReturnedCoins);
                Assert.Equal(3, result.ReturnedCoins.Count);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.OneEuro.ToString() && c.Value == 1);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.TwentyCent.ToString() && c.Value == 2);
                Assert.Contains(result.ReturnedCoins, c => c.Key == CoinType.TenCent.ToString() && c.Value == 1);

                Assert.Equal(1, firstItem.Quantity);
                Assert.Equal(6, dbOneEuro.Quantity);
                Assert.Equal(0, dbFiftyCent.Quantity);
                Assert.Equal(6, dbTwentyCent.Quantity);
                Assert.Equal(6, dbTenCent.Quantity);

                #endregion
            }
            finally
            {
                #region Cleaning Cache

                TestUtilities.CleanCache(_cache);
                await TestUtilities.CleanData(_context);

                #endregion
            }
        }

        #endregion
    }
}
