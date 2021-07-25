using MediatR;
using Outhink.ResponseModels.QueryResponseModels;
using System.Collections.Generic;

namespace Outhink.RequestModels.QueryRequestModels
{
    /// <summary>
    /// Request model received when we want to get list of items
    /// </summary>
    public class GetItemsRequestModel : IRequest<List<GetItemsResponseModel>>
    {
        /// <summary>
        /// Number of items to skip (If needed later)
        /// </summary>
        public int Skip { get; set; } = 0;

        /// <summary>
        /// Number of items to take (If needed later)
        /// </summary>
        public int Take { get; set; } = 10;
    }
}
