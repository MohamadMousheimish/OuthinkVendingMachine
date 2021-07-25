namespace Outhink.ResponseModels.QueryResponseModels
{
    /// <summary>
    /// Response model sent when fetching a list of items
    /// </summary>
    public class GetItemsResponseModel
    {
        /// <summary>
        /// Id of the Item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Price of the item
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Quantity left of each item
        /// </summary>
        public int Quantity { get; set; }
    }
}
