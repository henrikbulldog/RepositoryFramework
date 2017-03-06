using System.Collections.Generic;
using System.Linq;
using Xunit;
using RepositoryFramework.Test.Models;
using System;
using System.Diagnostics;
using RepositoryFramework.EntityFramework;

namespace RepositoryFramework.Test
{
	public class RepositoryTest
	{
		[Fact]
		public void EFCore_Should_Not_Support_Lazy_Load()
		{
			// Create new empty database
			using (var db = new SQLiteContext())
			{
				// Arrange
				var cr = new Repository<Category>(db);
				var c = CreateCategory(100);
				cr.Create(c);
				cr.SaveChanges();

				// Detach all to avoid expansions already cached in the context
				cr.DetachAll();

				// Act
				var cdb = cr.Find().Items.First();
				Assert.NotNull(cdb);

				// Assert
				// Looks like lazy loading is not supported in EF Core
				Assert.Null(cdb.Products);
			}
		}

		[Fact]
		public void EFCore_Should_Support_Circular_References()
		{
			// Create new empty database
			using (var db = new SQLiteContext())
			{
				// Arrange
				var cr = new Repository<Category>(db);
				var c = CreateCategory(100);
				cr.Create(c);
				cr.SaveChanges();

				// Detach all to avoid expansions already cached in the context
				cr.DetachAll();

				// Act
				var constraint = new QueryConstraints<Category>();
				constraint.Include(new List<string> { "Products.Parts.Product" });
				var cdb = cr.Find(constraint).Items.First();
				Assert.NotNull(cdb);
				var products = cdb.Products;

				// Assert
				Assert.NotNull(products);
				Assert.Equal(100, products.Count);
				var product = products.First();
				for(int i = 0; i < 10; i++)
				{
					Assert.NotNull(product);
					Assert.NotNull(product.Parts);
					var part = product.Parts.First();
					product = part.Product;
				}
			}
		}

		[Theory]
		[InlineData("", 100, 0, 0)]
		[InlineData("Products", 100, 100, 0)]
		[InlineData("pRoducts", 100, 100, 0)]
		[InlineData("Products.Parts", 100, 100, 100)]
		public void Include(string includes, int productRows, int expectedProductRows, int expectedPartRows)
		{
			// Create new empty database
			using (var db = new SQLiteContext())
			{
				// Arrange
				var categoryRepository = new Repository<Category>(db);
				var category = CreateCategory(productRows);
				categoryRepository.Create(category);
				categoryRepository.SaveChanges();

				// Detach all to avoid expansions already cached in the context
				categoryRepository.DetachAll();

				// Act
				var includeList = includes.Split(',');
				var constraint =
					includeList != null && includeList.Length > 1
					? new QueryConstraints<Category>().Include(new List<string>(includeList))
					: new QueryConstraints<Category>().Include(includes);

				category = categoryRepository.Find(constraint).Items.First();
				Assert.NotNull(category);
				var products = category.Products;

				// Assert
				if (products == null)
				{
					Assert.Equal(0, expectedProductRows);
				}
				else
				{
					Assert.Equal(expectedProductRows, products.Count);
					var product = products.First();
					if (product.Parts == null)
					{
						Assert.Equal(0, expectedPartRows);
					}
					else
					{
						Assert.Equal(expectedPartRows, product.Parts.Count);
					}
				}
			}
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public void SortBy(bool descendingOrder, bool useExpression)
		{
			// Create new empty database
			using (var db = new SQLiteContext())
			{
				// Arrange
				var cr = new Repository<Category>(db);
				var c = CreateCategory(100);
				cr.Create(c);
				cr.SaveChanges();

				// Act
				var constraints = descendingOrder
					? (useExpression
						? new QueryConstraints<Product>().SortByDescending(p => p.Name)
						: new QueryConstraints<Product>().SortByDescending("Name"))
					: (useExpression
						? new QueryConstraints<Product>().SortBy(p => p.Name)
						: new QueryConstraints<Product>().SortBy("Name"));
				var pr = new Repository<Product>(db);
				var products = pr.Find(constraints.Include("Parts"))
					.Items.ToList();
				Assert.NotNull(products);

				// Assert
				var sortedproducts = descendingOrder
					? new List<Product>(products).OrderByDescending(p => p.Name)
					: new List<Product>(products).OrderBy(p => p.Name);
				var i = 0;
				foreach(var p in sortedproducts)
				{
					//Console.WriteLine($"{p.Name}\t{products[i++].Name}");
					Assert.Equal(p, products[i++]);
				}
			}
		}


		[Theory]
		[InlineData(1, 40, 100, 40)]
		[InlineData(2, 40, 100, 40)]
		[InlineData(3, 40, 100, 20)]
		[InlineData(4, 40, 100, 0)]
		public void Page(int page, int pageSize, int totalRows, int expectedRows)
		{
			// Create new empty database
			using (var db = new SQLiteContext())
			{
				// Arrange
				var cr = new Repository<Category>(db);
				var category = CreateCategory(totalRows);
				cr.Create(category);
				cr.SaveChanges();

				// Act
				var pr = new Repository<Product>(db);
				var pageItems = pr.Find(
					new QueryConstraints<Product>()
					.Page(page, pageSize)).Items;

				// Assert
				Assert.NotNull(pageItems);
				Assert.Equal(expectedRows, pageItems.Count());
			}
		}

		[Fact]
		public void Combine_Page_Sort_Include()
		{
			// Create new empty database
			using (var db = new SQLiteContext())
			{
				// Arrange
				var cr = new Repository<Category>(db);
				var category = CreateCategory(100);
				cr.Create(category);
				cr.SaveChanges();

				// Act
				var pr = new Repository<Product>(db);
				var pageItems = pr.Find(
					new QueryConstraints<Product>()
					.Page(3, 40)
					.SortBy("Name")
					.Include(new List<string> { "Parts" })
					).Items;

				// Assert
				Assert.NotNull(pageItems);
				Assert.Equal(20, pageItems.Count());
			}
		}

		private Product CreateProduct(Category c, int partRows=100)
		{
			var p = new Product
			{
				Name = Guid.NewGuid().ToString().ToLower().Replace("aa", "ab"),
				Category = c,
				Description = Guid.NewGuid().ToString(),
				Price = 1.23M
			};
			var l = new List<Part>();
			for (int i = 0; i < partRows; i++)
			{
				l.Add(CreatePart(p));
			};
			p.Parts = l;
			return p;
		}

		private Part CreatePart(Product p)
		{
			return new Part
			{
				Name = Guid.NewGuid().ToString().ToLower().Replace("aa", "ab"),
				Product = p,
				Description = Guid.NewGuid().ToString(),
				Price = 1.23M
			};
		}

		private Category CreateCategory(int productRows = 100)
		{
			var c = new Category
			{
				Name = Guid.NewGuid().ToString(),
				Description = Guid.NewGuid().ToString()
			};
			var products = new List<Product>();
			for (int i = 0; i < productRows; i++)
			{
				products.Add(CreateProduct(c));
			};
			c.Products = products;
			return c;
		}
	}
}
