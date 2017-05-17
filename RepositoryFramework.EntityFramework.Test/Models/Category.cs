using System;
using System.Collections.Generic;

namespace RepositoryFramework.Test.Models
{
  public class Category
    {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string NullField { get; set; } = null;
    public DateTime? DateTimeField { get; set; } = DateTime.Now;
    public virtual ICollection<Product> Products { get; set; }
  }
}
