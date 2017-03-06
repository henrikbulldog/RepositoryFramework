using System;
using System.Collections.Generic;

namespace RepositoryFramework.Test.Models
{
	public class Order
    {
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
  }
}
