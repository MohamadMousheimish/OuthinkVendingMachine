using Outhink.Db.Enums;
using System.Collections.Generic;

namespace Outhink.ResponseModels.CommandResponseModels
{
    /// <summary>
    /// Response model sent when an order is canceled
    /// </summary>
    public class CancelOrderResponseModel
    {
        /// <summary>
        /// Coins returned to the user
        /// </summary>
        /// <remarks>
        /// <see cref="string"/> Indicate the type of coin
        /// <see cref="int"/> Indicate the amount of coin inserted
        /// </remarks>
        public Dictionary<string, int> Coins { get; set; } = new Dictionary<string, int>();
    }
}
