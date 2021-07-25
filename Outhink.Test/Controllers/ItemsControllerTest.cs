using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Outhink.Controllers;
using Outhink.RequestModels.QueryRequestModels;
using Outhink.ResponseModels.QueryResponseModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Outhink.Test.Controllers
{
    public class ItemsControllerTest
    {
        private readonly Mock<IMediator> _mediator;
        private ItemsController _controller;
        private List<GetItemsResponseModel> _items;

        public ItemsControllerTest()
        {
            _mediator = new Mock<IMediator>();
            _items = new List<GetItemsResponseModel>
            {
                TestUtilities.CreateItemsResponseModel(1, "Juice", 1.8),
                TestUtilities.CreateItemsResponseModel(2, "Espresso", 1.2),
            };
            SetMediatorMethods();
            _controller = new ItemsController(_mediator.Object);
        }

        /// <summary>
        /// Unit test which will call the get items API
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetItemsTest()
        {
            #region Arrange

            GetItemsRequestModel requestModel = new();

            #endregion

            #region Act

            var result = await _controller.GetItems(requestModel);

            #endregion

            #region Assert

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var okObjectResult = result as OkObjectResult;
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.NotNull(okObjectResult.Value);
            Assert.IsType<List<GetItemsResponseModel>>(okObjectResult.Value);
            var returnedItems = okObjectResult.Value as List<GetItemsResponseModel>;
            Assert.Equal(2, returnedItems.Count);
            var juiceItem = returnedItems.FirstOrDefault(i => i.Name == "Juice");
            Assert.NotNull(juiceItem);
            Assert.Equal(1.8, juiceItem.Price);

            #endregion
        }

        #region Private Methods

        private void SetMediatorMethods()
        {
            _mediator.Setup(m => m.Send(It.IsAny<GetItemsRequestModel>(), default)).ReturnsAsync(_items);
        }

        #endregion

    }
}
