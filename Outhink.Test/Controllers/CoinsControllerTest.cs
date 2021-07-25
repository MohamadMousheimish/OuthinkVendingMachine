using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Outhink.Controllers;
using Outhink.Db.Enums;
using Outhink.RequestModels.CommandRequestModels;
using Outhink.RequestModels.QueryRequestModels;
using Outhink.ResponseModels.CommandResponseModels;
using Outhink.ResponseModels.QueryResponseModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Outhink.Test.Controllers
{
    public class CoinsControllerTest
    {
        private readonly Mock<IMediator> _mediator;
        private CoinsController _controller;
        private List<GetCoinsResponseModel> _coins;
        public CoinsControllerTest()
        {
            _mediator = new Mock<IMediator>();
            _coins = new List<GetCoinsResponseModel>
            {
                TestUtilities.CreateCoinsResponseModel(1, CoinType.FiftyCent.ToString()),
                TestUtilities.CreateCoinsResponseModel(15, CoinType.TwentyCent.ToString()),
                TestUtilities.CreateCoinsResponseModel(10, CoinType.OneEuro.ToString())
            };
            SetMediatorMethods();
            _controller = new CoinsController(_mediator.Object);
        }

        /// <summary>
        /// Unit test which will call the get coins API
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetCoinsTest()
        {
            #region Arrange

            GetCoinsRequestModel requestModel = new();

            #endregion

            #region Act

            var result = await _controller.GetVendingMachineCoins(requestModel);

            #endregion

            #region Assert

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var okObjectResult = result as OkObjectResult;
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.NotNull(okObjectResult.Value);
            Assert.IsType<List<GetCoinsResponseModel>>(okObjectResult.Value);
            var returnedCoins = okObjectResult.Value as List<GetCoinsResponseModel>;
            Assert.Equal(3, returnedCoins.Count);
            var oneEuroCoins = returnedCoins.FirstOrDefault(c => c.Type == CoinType.OneEuro.ToString());
            Assert.Equal(10, oneEuroCoins.Quantity);

            #endregion
        }

        /// <summary>
        /// Unit test which will call the insert coin API
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertCoinsTest_Succeeded()
        {
            #region Arrange

            InsertCoinsRequestModel requestModel = new()
            {
                Type = CoinType.FiftyCent
            };

            #endregion

            #region Act

            var result = await _controller.InsertCoins(requestModel);

            #endregion

            #region Assert

            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);
            var okObjectResult = result as OkResult;
            Assert.Equal(200, okObjectResult.StatusCode);

            #endregion
        }

        #region Private Methods

        private void SetMediatorMethods()
        {
            _mediator.Setup(m => m.Send(It.IsAny<GetCoinsRequestModel>(), default)).ReturnsAsync(_coins);
            _mediator.Setup(m => m.Send(It.Is<InsertCoinsRequestModel>(requestModel => requestModel.Type >= 0), default)).ReturnsAsync(new InsertCoinsResponseModel
            {
                Succeeded = true
            });
        }

        #endregion
    }
}
