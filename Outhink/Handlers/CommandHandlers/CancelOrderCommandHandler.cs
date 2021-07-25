using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Outhink.Helpers;
using Outhink.RequestModels.CommandRequestModels;
using Outhink.ResponseModels.CommandResponseModels;
using System.Threading;
using System.Threading.Tasks;

namespace Outhink.Handlers.CommandHandlers
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderRequestModel, CancelOrderResponseModel>
    {
        private readonly IMemoryCache _cache;

        public CancelOrderCommandHandler(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<CancelOrderResponseModel> Handle(CancelOrderRequestModel request, CancellationToken cancellationToken)
        {
            CancelOrderResponseModel responseModel = new();
            GeneralUtilities.CleanCache(_cache, responseModel.Coins);
            return responseModel;
        }
    }
}
