using AutoMapper;
using MediatR;
using Outhink.Db.Models;
using Outhink.Db.Repositories;
using Outhink.RequestModels.QueryRequestModels;
using Outhink.ResponseModels.QueryResponseModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Outhink.Handlers.QueryHandlers
{
    public class GetCoinsHandler : IRequestHandler<GetCoinsRequestModel, List<GetCoinsResponseModel>>
    {
        private readonly IBaseRepository<Coin> _coinRepository;
        private readonly IMapper _mapper;

        public GetCoinsHandler(IBaseRepository<Coin> coinRepository, IMapper mapper)
        {
            _coinRepository = coinRepository;
            _mapper = mapper;
        }

        public async Task<List<GetCoinsResponseModel>> Handle(GetCoinsRequestModel request, CancellationToken cancellationToken)
        {
            var coins = await _coinRepository.ListAllAsync();
            List<GetCoinsResponseModel> responseModels = _mapper.Map<List<GetCoinsResponseModel>>(coins);
            return responseModels;
        }
    }
}
