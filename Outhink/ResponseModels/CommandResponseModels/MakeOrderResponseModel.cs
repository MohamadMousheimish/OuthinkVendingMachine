using System.Collections.Generic;

namespace Outhink.ResponseModels.CommandResponseModels
{
    /// <summary>
    /// Response model sent when an order is made
    /// </summary>
    public class MakeOrderResponseModel
    {
        /// <summary>
        /// Message sent when a purshase order is done
        /// </summary>
        public string Note { get; set; } = "Thank you";

        /// <summary>
        /// Flag to indicate if the purshase order is successed or not
        /// </summary>
        public bool Succeeded { get; set; } = true;

        /// <summary>
        /// The returned coins
        /// </summary>
        public Dictionary<string, int> ReturnedCoins { get; set; } = new Dictionary<string, int>();
    }
}
