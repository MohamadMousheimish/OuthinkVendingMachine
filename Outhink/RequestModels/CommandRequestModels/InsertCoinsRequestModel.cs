using MediatR;
using Outhink.Db.Enums;
using Outhink.ResponseModels.CommandResponseModels;

namespace Outhink.RequestModels.CommandRequestModels
{
    public class InsertCoinsRequestModel : IRequest<InsertCoinsResponseModel>
    {
        /// <summary>
        /// Coin Type
        /// </summary>
        /// <see cref="CoinType"/>
        public CoinType Type { get; set; }
    }
}
