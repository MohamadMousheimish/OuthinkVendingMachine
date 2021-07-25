namespace Outhink.ResponseModels.QueryResponseModels
{
    /// <summary>
    /// Response model sent when fetching a list of coins
    /// </summary>
    public class GetCoinsResponseModel
    {
        /// <summary>
        /// Type of the coin returned
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Quantity left in the vending machine
        /// </summary>
        public int Quantity { get; set; }
    }
}
