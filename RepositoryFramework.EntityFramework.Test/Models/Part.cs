namespace RepositoryFramework.Test.Models
{
	public class Part
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public virtual Product Product { get; set; }
  }
}
