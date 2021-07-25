using AutoMapper;
using MediatR;
using Outhink.Db.Models;
using Outhink.Db.Repositories;
using Outhink.RequestModels.QueryRequestModels;
using Outhink.ResponseModels.QueryResponseModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Outhink.Handlers.QueryHandlers
{
    public class GetItemsHandler : IRequestHandler<GetItemsRequestModel, List<GetItemsResponseModel>>
    {
        private readonly IBaseRepository<Item> _itemRepository;
        private readonly IMapper _mapper;
        public GetItemsHandler(IBaseRepository<Item> itemRepository, IMapper mapper)
        {
            _itemRepository = itemRepository;
            _mapper = mapper;
        }

        public async Task<List<GetItemsResponseModel>> Handle(GetItemsRequestModel request, CancellationToken cancellationToken)
        {
            var items = await _itemRepository.ListAllAsync();
            items = items.Skip(request.Skip)
                .Take(request.Take);
            List<GetItemsResponseModel> responseModels = _mapper.Map<List<GetItemsResponseModel>>(items);
            return responseModels;
        }
    }
}
