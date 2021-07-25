using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Outhink.RequestModels.CommandRequestModels;
using Outhink.ResponseModels.CommandResponseModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outhink.Handlers.CommandHandlers
{
    public class InsertCoinsCommandHandler : IRequestHandler<InsertCoinsRequestModel, InsertCoinsResponseModel>
    {
        private readonly IMemoryCache _cache;

        public InsertCoinsCommandHandler(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<InsertCoinsResponseModel> Handle(InsertCoinsRequestModel request, CancellationToken cancellationToken)
        {
            InsertCoinsResponseModel responseModel = new();
            try
            {
                var key = request.Type.ToString();
                var isTypeCached = _cache.TryGetValue(key, out int value);

                //Check if coin is inserted before by the user
                if (isTypeCached)
                {
                    //If inserted before, get old value, incremenet and re-insert the new value
                    value++;
                    _cache.Remove(key);
                    _cache.Set(key, value);
                }
                else
                {
                    //If not inserted, one coin of the inserted type is added
                    _cache.Set(key, 1);
                }
            }
            catch (Exception)
            {
                responseModel.Succeeded = false;
            }
            return responseModel;
        }
    }
}
