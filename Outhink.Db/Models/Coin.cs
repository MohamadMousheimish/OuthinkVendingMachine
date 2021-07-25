using Outhink.Db.Enums;

namespace Outhink.Db.Models
{
    public class Coin : BaseEntity
    {
        /// <summary>
        /// How much left of the coin in the vending machine
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Type of the coin
        /// </summary>
        /// <example>Euro, Cent</example>
        /// <see cref="CoinType"/>
        public CoinType Type { get; set; }
    }
}
