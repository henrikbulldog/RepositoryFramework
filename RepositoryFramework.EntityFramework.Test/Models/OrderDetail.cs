namespace RepositoryFramework.Test.Models
{
  public class OrderDetail
    {
    public int Id { get; set; }

    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public virtual Product Product { get; set; }
    public virtual Order Order { get; set; }
  }
}
