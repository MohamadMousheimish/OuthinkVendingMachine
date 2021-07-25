using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Outhink.Controllers;
using Outhink.Db.Enums;
using Outhink.RequestModels.CommandRequestModels;
using Outhink.ResponseModels.CommandResponseModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Outhink.Test.Controllers
{
    public class OrdersControllerTest
    {
        private readonly Mock<IMediator> _mediator;
        private OrdersController _controller;

        public OrdersControllerTest()
        {
            _mediator = new Mock<IMediator>();
            SetMediatorMethods();
            _controller = new OrdersController(_mediator.Object);
        }

        /// <summary>
        /// Unit test which will call the buy order API
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SubmitOrderTest()
        {
            #region Arrange

            MakeOrderRequestModel requestModel = new();

            #endregion

            #region Act

            var result = await _controller.BuyOrder(requestModel);

            #endregion

            #region Assert

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var okObjectResult = result as OkObjectResult;
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.NotNull(okObjectResult.Value);
            Assert.IsType<MakeOrderResponseModel>(okObjectResult.Value);
            var returnedValue = okObjectResult.Value as MakeOrderResponseModel;
            Assert.Equal("Thank you", returnedValue.Note);
            Assert.Single(returnedValue.ReturnedCoins);
            var returnedCoin = returnedValue.ReturnedCoins.FirstOrDefault();
            Assert.Equal(CoinType.FiftyCent.ToString(), returnedCoin.Key);
            Assert.Equal(10, returnedCoin.Value);

            #endregion
        }

        /// <summary>
        /// Unit test which will call the cancel order API
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CancelOrderTest()
        {
            #region Arrange

            CancelOrderRequestModel requestModel = new();

            #endregion

            #region Act

            var result = await _controller.CancelOrder(requestModel);

            #endregion

            #region Assert

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var okObjectResult = result as OkObjectResult;
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.NotNull(okObjectResult.Value);
            Assert.IsType<CancelOrderResponseModel>(okObjectResult.Value);
            var returnedValue = okObjectResult.Value as CancelOrderResponseModel;
            var returnedCoin = returnedValue.Coins.FirstOrDefault();
            Assert.Equal(CoinType.OneEuro.ToString(), returnedCoin.Key);
            Assert.Equal(15, returnedCoin.Value);

            #endregion
        }

        #region Private Methods

        private void SetMediatorMethods()
        {
            _mediator.Setup(m => m.Send(It.IsAny<MakeOrderRequestModel>(), default)).ReturnsAsync(new MakeOrderResponseModel
            {
                ReturnedCoins = new Dictionary<string, int>
                {
                    { CoinType.FiftyCent.ToString(), 10}
                }
            });

            _mediator.Setup(m => m.Send(It.IsAny<CancelOrderRequestModel>(), default)).ReturnsAsync(new CancelOrderResponseModel
            {
                Coins = new Dictionary<string, int>
                {
                    { CoinType.OneEuro.ToString(), 15}
                }
            });
        }

        #endregion

    }
}
