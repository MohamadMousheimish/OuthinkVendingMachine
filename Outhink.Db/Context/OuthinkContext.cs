using Microsoft.EntityFrameworkCore;
using Outhink.Db.Models;

namespace Outhink.Db.Context
{
    public class OuthinkContext : DbContext
    {
        public OuthinkContext(DbContextOptions<OuthinkContext> options) : base(options)
        {
        }

        public DbSet<Coin> Coins { get; set; }
        public DbSet<Item> Items { get; set; }
    }
}
