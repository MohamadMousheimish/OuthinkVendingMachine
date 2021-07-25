using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Outhink.Db.Context;
using Outhink.Db.Models;
using Outhink.Db.Repositories;
using Outhink.Handlers.QueryHandlers;
using System.Threading.Tasks;
using Xunit;


namespace Outhink.Test.Handlers.QueryHandlers
{
    public class GetItemsHandlerTest
    {
        private readonly GetItemsHandler _handler;
        private readonly IMapper _mapper;
        private readonly IBaseRepository<Item> _repository;
        private readonly OuthinkContext _context;

        public GetItemsHandlerTest()
        {
            var options = new DbContextOptionsBuilder<OuthinkContext>()
                .UseInMemoryDatabase(databaseName: "GetItemsHandlerTest")
                .Options;
            _context = new OuthinkContext(options);
            _repository = new BaseRepository<Item>(_context);
            _mapper = TestUtilities.CreateMapper();
            _handler = new GetItemsHandler(_repository, _mapper);
        }

        /// <summary>
        /// Unit test which will call the get item handler
        /// </summary>
        [Fact]
        public async Task GetItemsTest()
        {
            try
            {
                #region Arrange

                var juiceName = "Juice";
                var juicePrice = 1.3;
                var juiceQuantity = 10;
                var espressoName = "Espresso";
                var espressoPrice = 1.8;
                var espressoQuantity = 15;
                var firstItem = TestUtilities.CreateItem(juiceName, juicePrice, juiceQuantity);
                var secondItem = TestUtilities.CreateItem(espressoName, espressoPrice, espressoQuantity);
                await _repository.AddAsync(firstItem);
                await _repository.AddAsync(secondItem);
                var requestModel = TestUtilities.CreateItemsRequestModel();

                #endregion

                #region Act

                var items = await _handler.Handle(requestModel, default);

                #endregion

                #region Assert

                Assert.NotNull(items);
                Assert.Equal(2, items.Count);
                Assert.Contains(items, c => c.Name == juiceName && c.Price == juicePrice);
                Assert.Contains(items, c => c.Name == espressoName && c.Price == espressoPrice);

                #endregion
            }
            finally
            {
                await TestUtilities.CleanData(_context);
            }
        }
    }
}
