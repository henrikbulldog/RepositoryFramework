using Microsoft.EntityFrameworkCore;
using RepositoryFramework.Test.Models;

namespace RepositoryFramework.Test
{
  public class SQLiteContext : DbContext
  {
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public SQLiteContext() : base(
      new DbContextOptionsBuilder<SQLiteContext>()
        .UseSqlite($"Filename=./{System.Guid.NewGuid().ToString()}.db")
        .Options)
    {
      Database.EnsureDeleted();
      Database.EnsureCreated();
    }
    public override void Dispose()
    {
      Database.EnsureDeleted();
      base.Dispose();
    }
  }
}
