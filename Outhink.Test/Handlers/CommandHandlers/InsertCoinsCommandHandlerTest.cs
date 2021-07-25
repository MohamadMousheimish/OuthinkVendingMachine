using Microsoft.Extensions.Caching.Memory;
using Outhink.Db.Enums;
using Outhink.Handlers.CommandHandlers;
using System.Threading.Tasks;
using Xunit;

namespace Outhink.Test.Handlers.CommandHandlers
{
    public class InsertCoinsCommandHandlerTest
    {
        private readonly IMemoryCache _cache;
        private readonly InsertCoinsCommandHandler _handler;

        public InsertCoinsCommandHandlerTest()
        {
            _cache = TestUtilities.CreateTestingMemoryCache();
            _handler = new InsertCoinsCommandHandler(_cache);
        }

        /// <summary>
        /// Unit test which will call the insert coin handler
        /// </summary>
        [Fact]
        public async Task InsetCoinTest()
        {
            var fiftyCentCoinType = CoinType.FiftyCent;
            var oneEuroCoinType = CoinType.OneEuro;
            try
            {
                #region Arrange

                var fiftyCentRequestModel = TestUtilities.CreateInsertCoinsRequestModel(fiftyCentCoinType);
                var oneEuroRequestModel = TestUtilities.CreateInsertCoinsRequestModel(oneEuroCoinType);

                #endregion

                #region Act

                var firstResult = await _handler.Handle(fiftyCentRequestModel, default);
                var secondResult = await _handler.Handle(fiftyCentRequestModel, default);
                var ThirdResult = await _handler.Handle(oneEuroRequestModel, default);
                var fiftyCentCacheExist = _cache.TryGetValue(fiftyCentCoinType.ToString(), out int fiftyCentQuantity);
                var oneEuroCacheExist = _cache.TryGetValue(oneEuroCoinType.ToString(), out int oneEuroQuantity);

                #endregion

                #region Assert

                Assert.NotNull(firstResult);
                Assert.True(firstResult.Succeeded);
                Assert.True(fiftyCentCacheExist);
                Assert.True(oneEuroCacheExist);
                Assert.Equal(2, fiftyCentQuantity);
                Assert.Equal(1, oneEuroQuantity);

                #endregion
            }
            finally
            {
                if (_cache.TryGetValue(fiftyCentCoinType, out int _))
                {
                    _cache.Remove(fiftyCentCoinType);
                }
            }
        }

    }
}
