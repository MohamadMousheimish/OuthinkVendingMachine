using MediatR;
using Outhink.ResponseModels.CommandResponseModels;

namespace Outhink.RequestModels.CommandRequestModels
{
    /// <summary>
    /// Model received when we want to buy an item
    /// </summary>
    public class MakeOrderRequestModel : IRequest<MakeOrderResponseModel>
    {
        /// <summary>
        /// Id of the item we want to purshase
        /// </summary>
        public int ItemId { get; set; }
    }
}
