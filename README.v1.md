# RepositoryFramework
A .Net framework for accessing data using the Repository pattern.
I used the code and advice given by Jonas Gauffin in this article: https://dzone.com/articles/repository-pattern-done-right, and elaborated on that.

## What is the "Repository Pattern" ?
I will refer you to this excellent article instead of venturing an explanation myself: https://martinfowler.com/eaaCatalog/repository.html!

## What is the "Repository Framework" ?
A collection of generic interfaces and utility classes that abstracts concrete implementations of repositories.
Currently there are 3 implementations of the interfaces in separate packages on NuGet.org:
* RepositoryFramework.EntityFramework. 
  * Uses Entity Framework Core, see https://docs.microsoft.com/en-us/ef/core/
  * No SQL needed
* RepositoryFramework.Dapper
  * Uses Daper, see https://github.com/StackExchange/Dapper
  * Uses dynamic SQL embedded in C# code
* RepositoryFramework.Api
  *  Uses RestSharp, see http://restsharp.org/
  *  For ReSTful API clients
*  RepositoryFramework.MongoDB
   *  Uses MongoDB, see https://docs.mongodb.com/
   *  Uses the MongoDB C# driver, https://github.com/mongodb/mongo-csharp-driver

## Why Should I Use This Repository Framework ?
You should't necessarily. Every tool has its purpose. 
If you are writing a simple application with a limited functional scope, you should use your data access framework directly. 
Don't bother setting up repositories unless you really need them. 
If you are building Microservices as part of a larger enterprise scale solution, streamlining your data access code, through the use of the Repository Framework, might turn out to be a good investment;
Simply because the code base will be easier to read and navigate.

### Interfaces for Repositories that Support Linq to SQL
These interfaces are typically used by repositories that support Linq to SQL, they are used in RepositoryFramework.EntityFramework:
* IRepository: Creates, updates and deletes entities
* IUnitOfWork: Saves changes made to a database context
* IGetQueryable: Gets an entity by a filter expression and query constraints for expanding (eager loading) related objects
* IFind: Finds a list of entites
* IFindQueryable: Finds a list of entites using a filter expression and query constraints for expansion, paging and sorting

### Interfaces for Repositories with No Support for Linq to SQL
These interfaces are used by repositories with no support for Linq to SQL, they are used in RepositoryFramework.Api and RepositoryFramework.Dapper:
* IRepository: Creates, updates and deletes entities
* IGet: Gets an entity by id
* IFind: Finds a list of entites
* IFindFilter: Finds a list of entites using an object with filtering information

## I See a lot of Interfaces, Why Not Just IRepository ?
Well, I decided that the Repository Framework should support these scenarios, because that was what I needed in my current project:
* A repository that supports the classical CRUD operations: Create, Read, Update and Delete.
* A repository that only supports a subset of CRUD, for example a read-only repository.
* A repository that supports Layered Executrion Trees (LET) or Linq to SQL, such as Entity Framework.
* A repository that does not support LET, such as a repository that calls a legacy data access framework or a downstream API.
* A repository that supports data paging and sorting.

## How To Use RepositoryFramework.EntityFramework ?
  ~~~~
  // Given this model:
	
  public class Category
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public virtual ICollection<Product> Products { get; set; }
  }

  public class Product
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public virtual Category Category { get; set; }
    public ICollection<Part> Parts { get; set; }
  }

  ...

  // To create an entity:
  DbContext db = CreateContext();
  var categoryRepository = new Repository<Category>(db);
  var category = new Category { Name = "Category1" };
  categoryRepository.Create(category);
  categoryRepository.SaveChanges();

  ...

  // To update an entity:
  category.Name = "Changed name";
  categoryRepository.Update(category);
  categoryRepository.SaveChanges();
  ...

  // To delete an entity:
  categoryRepository.Delete(category);
  categoryRepository.SaveChanges();

  ...

  // To read an entity with Queryable lambda filter and eager loading:
  categoryRepository.GetById(() => (cat => cat.Id == 123)
    .Include("Products"));

  ...

  // To read a list of entities with Queryable lambda filter, eager loading, sorting and paging:
  categoryRepository.Find(() => (cat => cat.Id < 100),
    constraints
      .SortBy(p => p.Name)
      .Page(1, 50)
      .Include("Products"));

  ~~~~

### Wait, You Shouldn't Expose Linq from a Repository!
True in principle, because Linq to SQL implementations are incomplete and differ from one ORM framework to another. 
If you don't like Linq parameters, inherit from Repository and do your own implementation:

~~~~
  public class CategoryRepository : Repository<Category>
  {
    Category GetById(int id)
    {
      return GetById(() => (cat => cat.Id == id);
    }
  }
~~~~

## How To Use RepositoryFramework.Dapper?
  ~~~~
  // Given this model:
	
  public class Category
  {
    public int Id? { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public virtual ICollection<Product> Products { get; set; }
  }

  public class Product
  {
    public int Id? { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public virtual Category Category { get; set; }
    public ICollection<Part> Parts { get; set; }
  }

  public class IdFilter
  {
    public int Id { get; set; }
  }

  public class CategoryFilter
  {
    public string Name { get; set; }
    public string Description { get; set; }
  }

  ...

  // To create an entity:
  IDbConnection connection = CreateConnection();
  var categoryRepository = new Repository<Category, IdFilter>(connection);
  var category = new Category
  {
    Name = Guid.NewGuid().ToString(),
    Description = Guid.NewGuid().ToString()
  };
  categoryRepository.Create(category);

  ...

  // To update an entity:
  category.Name = "Changed name";
  categoryRepository.Update(category);
  categoryRepository.SaveChanges();

  ...

  // To delete an entity:
  categoryRepository.Delete(category);
  categoryRepository.SaveChanges();

  ...

  // To read an entity with a filter:
  var categoryRepository = CreateCategoryRepository<CategoryFilter>(connection);
  var result = categoryRepository.Find(
    new CategoryFilter 
    { 
      Name = "Category 1"
    });

  ...

  // To expand replated objects, you have to override the Find() method:
  public class CategoryRepository : Repository<Category, CategoryFilter>
  {
    public override IQueryResult<Category> Find()
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var findQuery = $@"
SELECT * FROM Category c
OUTER LEFT JOIN Product p ON p.CategoryId = c.Id";

      var lookup = new Dictionary<int?, Category>();
      IEnumerable<Category> result = SqlMapper.Query<Category, Product, Category>(
        Connection, 
        findQuery, 
        (category, product) => Map(category, product, lookup));

      var categories = lookup.Values.AsEnumerable();
      return new QueryResult<Category>(categories, categories.Count());
    }

    private Category Map(Category category, Product product, Dictionary<int?, Category> lookup)
    {
      Category currentCategory;

      if (!lookup.TryGetValue(category.Id, out currentCategory))
      {
        currentCategory = category;
        lookup.Add(category.Id, currentCategory);
      }

      if (currentCategory.Products == null)
      {
        currentCategory.Products = new List<Product>();
      }

      if (product != null)
      {
        currentCategory.Products.Add(product);
      }
      return currentCategory;
    }
  }

  ~~~~

### Wait, You Should Only Have One Language Per File!
I agree. So I made StoredProcedureRepository. This allows you to put all your SQL in the database and use stored procedures as an abstraction layer to SQL (and SQL dialects!).
In order to use StoredProcedureRepository<Category, TFilter>, you must create stored procedures using this naming convention:

* CREATE[Entity type name]
* INSERT[Entity type name]
* UPDATE[Entity type name]
* DELETE[Entity type name]

For example:

~~~~

CREATE PROCEDURE CreateCategory
  @Name NVARCHAR(100) = NULL,
  @Description NVARCHAR(100) = NULL
AS
BEGIN
  INSERT INTO Category (Name, Description)
  VALUES(@Name, @Description)

  SELECT @@IDENTITY
END

~~~~

Having created the stored procedures, you can use StoredProcedureRepository just like Repository.

## How To Use RepositoryFramework.Api?
To GET https://jsonplaceholder.typicode.com/posts:
~~~~
    private Configuration configuration =
      new Configuration
      {
        AuthenticationType = AuthenticationType.Anonymous
      };

      var apiRepository = new ApiRepository<Post>(configuration, 
        "https://jsonplaceholder.typicode.com");
      var result = apiRepository.Find();
~~~~
To POST https://jsonplaceholder.typicode.com/posts:
~~~~
    var post = new Post
      {
        Id = 1,
        UserId = 1,
        Title = "New title",
        Body = "New body"
      };
      apiRepository.Create(post);
~~~~
To PUT https://jsonplaceholder.typicode.com/posts/1:
~~~~
      apiRepository.Update(post);
~~~~
To DELETE https://jsonplaceholder.typicode.com/posts/1:
~~~~
      apiRepository.Delete(post);
~~~~
Tp specify GET parameters, alternative resource path and entity ID property:
~~~~
      var apiRepository = new ApiFilterRepository<Post, UserIdFilter>(
        configuration,
        "https://jsonplaceholder.typicode.com",
        "posts",
        (post => post.Id));
      var result = apiRepository.Find(new UserIdFilter { UserId = 1 });
~~~~

## How To Use RepositoryFramework.MongoDB
  ~~~~
  // Given this model:
	
  public class TestDocument
  {
    public string TestDocumentId { get; set; }

    public string StringTest { get; set; }

    public int IntTest { get; set; }
  }

  ...

  // To create an entity:
  mongoRepository = new MongoRepository<TestDocument>(GetDatabase(), 
    d =>
    {
      d.AutoMap();
      d.MapIdMember(c => c.TestDocumentId)
        .SetIdGenerator(StringObjectIdGenerator.Instance);
    });
  var doc = new TestDocument
  {
    StringTest = "A",
    IntTest = 1,
  };
  mongoRepository.Create(doc);

  ...

  // To read an entity by:
  var doc = categoryRepository.GetById("key");

  ...

  // To update an entity:
  mongoRepository.Update(doc);
  ...

  // To delete an entity:
  mongoRepository.Delete(doc);

  ...

  // To read a list of entities with a filter expression, sorting and paging:
  mongoRepository.Find(doc => doc.IntTest < 100),
      new QueryConstraints<TestDocument>()
        .SortBy(td => td.IntTest)
        .Page(1, 2));

  ~~~~
