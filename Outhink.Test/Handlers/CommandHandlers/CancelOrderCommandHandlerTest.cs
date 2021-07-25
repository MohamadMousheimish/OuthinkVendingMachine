using Microsoft.Extensions.Caching.Memory;
using Outhink.Db.Enums;
using Outhink.Handlers.CommandHandlers;
using Outhink.RequestModels.CommandRequestModels;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Outhink.Test.Handlers.CommandHandlers
{
    public class CancelOrderCommandHandlerTest
    {
        private readonly IMemoryCache _cache;
        private readonly CancelOrderCommandHandler _handler;
        public CancelOrderCommandHandlerTest()
        {
            _cache = TestUtilities.CreateTestingMemoryCache();
            _cache.Set(CoinType.FiftyCent.ToString(), 10);
            _handler = new CancelOrderCommandHandler(_cache);
        }

        /// <summary>
        /// Unit test which will call the cancel order handler
        /// </summary>
        [Fact]
        public async Task CancelOrderTest()
        {
            try
            {
                #region Arrange

                var requestModel = new CancelOrderRequestModel();

                #endregion

                #region Act

                var result = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.NotNull(result);
                Assert.NotNull(result.Coins);
                Assert.Single(result.Coins);
                var coin = result.Coins.FirstOrDefault();
                Assert.Equal(CoinType.FiftyCent.ToString(), coin.Key);
                Assert.Equal(10, coin.Value);

                #endregion
            }
            finally
            {
                //Cleaning cache if it wasn't cleaned
                if (_cache.TryGetValue(CoinType.FiftyCent, out int _))
                {
                    _cache.Remove(CoinType.FiftyCent);
                }
            }
        }
    }
}
