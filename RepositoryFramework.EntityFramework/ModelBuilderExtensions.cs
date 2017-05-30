using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// ModelBuilder extension methods
  /// </summary>
  public static class ModelBuilderExtensions
  {
    /// <summary>
    /// Avoid pluralizing table names convention
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
    {
      foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
      {
        entity.Relational().TableName = entity.ClrType.Name;
      }
    }
  }
}