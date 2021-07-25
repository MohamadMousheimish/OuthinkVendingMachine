using MediatR;
using Outhink.ResponseModels.QueryResponseModels;
using System.Collections.Generic;

namespace Outhink.RequestModels.QueryRequestModels
{
    public class GetCoinsRequestModel : IRequest<List<GetCoinsResponseModel>>
    {
        
    }
}
