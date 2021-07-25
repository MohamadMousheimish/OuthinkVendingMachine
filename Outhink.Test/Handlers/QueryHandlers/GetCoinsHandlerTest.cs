using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Outhink.Db.Context;
using Outhink.Db.Enums;
using Outhink.Db.Models;
using Outhink.Db.Repositories;
using Outhink.Handlers.QueryHandlers;
using System.Threading.Tasks;
using Xunit;

namespace Outhink.Test.Handlers.QueryHandlers
{
    public class GetCoinsHandlerTest
    {
        private readonly GetCoinsHandler _handler;
        private readonly IMapper _mapper;
        private readonly IBaseRepository<Coin> _repository;
        private readonly OuthinkContext _context;

        public GetCoinsHandlerTest()
        {
            var options = new DbContextOptionsBuilder<OuthinkContext>()
                .UseInMemoryDatabase(databaseName: "GetCoinsHandlerTest")
                .Options;
            _context = new OuthinkContext(options);
            _repository = new BaseRepository<Coin>(_context);
            _mapper = TestUtilities.CreateMapper();
            _handler = new GetCoinsHandler(_repository, _mapper);
        }

        /// <summary>
        /// Unit test which will call the get coin handler
        /// </summary>
        [Fact]
        public async Task GetCoinsTest()
        {
            try
            {
                #region Arrange

                var firstCoin = TestUtilities.CreateCoin(10, CoinType.FiftyCent);
                var secondCoin = TestUtilities.CreateCoin(15, CoinType.OneEuro);
                await _repository.AddAsync(firstCoin);
                await _repository.AddAsync(secondCoin);
                var requestModel = TestUtilities.CreateCoinsRequestModel();

                #endregion

                #region Act

                var coins = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.NotNull(coins);
                Assert.Equal(2, coins.Count);
                Assert.Contains(coins, c => c.Type == "One Euro" && c.Quantity == 15);
                Assert.Contains(coins, c => c.Type == "Fifty Cent" && c.Quantity == 10);

                #endregion
            }
            finally
            {
                await TestUtilities.CleanData(_context);
            }
        }

    }
}
