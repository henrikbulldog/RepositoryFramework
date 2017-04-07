using System.Collections.Generic;

namespace RepositoryFramework.Test.Models
{
  public class Category
    {
    public int? Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public virtual ICollection<Product> Products { get; set; }
  }
}
