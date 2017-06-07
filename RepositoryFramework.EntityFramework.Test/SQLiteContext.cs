using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryFramework.EntityFramework;
using RepositoryFramework.Test.Models;
using System;

namespace RepositoryFramework.Test
{
  public class SQLiteContext : DbContext
  {
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public SQLiteContext(ILogger logger= null)
    {
      Logger = logger;
      Database.EnsureDeleted();
      Database.EnsureCreated();
    }

    public ILogger Logger { get; set; }

    public override void Dispose()
    {
      try
      {
        Database.EnsureDeleted();
        base.Dispose();
      }
      catch { }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlite($"Filename=./{System.Guid.NewGuid().ToString()}.db");
      if (Logger != null)
      {
        var lf = new LoggerFactory();
        lf.AddProvider(new TestLoggerProvider(Logger));
        optionsBuilder.UseLoggerFactory(lf);
      }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.RemovePluralizingTableNameConvention();

      modelBuilder.Entity<Order>(entity =>
      {
        entity.HasKey(e => e.OrderKey);
      });
    }
  }
}
