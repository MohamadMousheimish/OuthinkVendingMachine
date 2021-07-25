namespace Outhink.Db.Models
{
    /// <summary>
    /// Item which will be sold in the vending machine
    /// </summary>
    public class Item : BaseEntity
    {
        /// <summary>
        /// Name of the Item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// How much of the Item is left
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Item Price
        /// </summary>
        public double Price { get; set; }
    }
}
