# RepositoryFramework
A .Net framework for accessing data using the Repository pattern.

## What is the "Repository Pattern" ?
I will refer you to this excellent article instead of venturing an explanation myself: https://martinfowler.com/eaaCatalog/repository.html!

## What is the "Repository Framework" ?
A collection of generic interfaces and utility classes that abstracts concrete implementations of repositories.

These methods are common to all repositories:
* Create() - Create a new entity
* CreateMany() - Create a collection of entities
* Update() - Update an existing entity
* Delete() - Delete an existing entity
* DeleteMany() - Delete a collection of entities
* GetById() - Get an entity from its id
* Find() - Get a collection of entities

Currently there are 4 implementations of the interfaces in separate packages on NuGet.org:

* RepositoryFramework.EntityFramework
  * Data access against a relational database using Entity Framework Core, see https://docs.microsoft.com/en-us/ef/core/
  * Generic repository EntityFramworkRepository
    * Additional methods:
      * Page() - Skip and limit result of Find()
      * SortBy() / SortBydescending() - Sort result of Find()
      * Include() - Include related entites to result of Find()
      * Find(Expression<Func<TEntity, bool>>) - Find with where expression
      * Find(string, IDictionary<string, object>) - Find with dynamic SQL
      * AsQueryable() - Gets a queryable collection of entities
* RepositoryFramework.Dapper
  * Data access against a relational database using Dapper micro-ORM, see https://github.com/StackExchange/Dapper
  * Generic repository DapperRepository
    * Uses Dapper with dynamic SQL
      * Addtional methods:
        * Page() - Skip and limit result of Find()
        * SortBy() / SortBydescending() - Sort result of Find()
        * Find(string, IDictionary<string, object>) - Find with dynamic SQL
  *  Generic repository StoredProcedureDapperRepository
     * Uses Dapper with stored procedures
      * Addtional methods:
        * SetParameter() - set parameters to stored procedures
* RepositoryFramework.Api
  * Data access against a ReSTful API using RestSharp, see http://restsharp.org/
  * Generic repository ApiRepository   
    * Additional methods:
      * SetParameter() - set parameters for API calls
*  RepositoryFramework.MongoDB
  *  Data access against a No-SQL document database using the MongoDB C# driver, see https://github.com/mongodb/mongo-csharp-driver
  * Generic repository MongoDBRepository   
    * Addtional methods:
      * Page() - Skip and limit result of Find()
      * SortBy() / SortBydescending() - Sort result of Find()
      * Find(string) - Find with a BSON filter definition
      * Find(Expression<Func<TEntity, bool>>) - Find with where expression
      * AsQueryable() - Gets a queryable collection of entities

## Why Should I Use This Repository Framework ?
You should't necessarily. Every tool has its purpose. 
If you are writing a simple application with a limited functional scope, you should use your data access framework directly. 
Don't bother setting up repositories unless you really need them. 
If you are building Microservices as part of a larger enterprise scale solution, streamlining your data access code, through the use of the Repository Framework, might turn out to be a good investment;
Simply because the code base will be easier to read and navigate.

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

  // To create an entity:
  DbContext db = CreateContext();
  var categoryRepository = new EntityFrameworkRepository<Category>(db);
  var category = new Category { Name = "Category1" };
  categoryRepository.Create(category);

  // To update an entity:
  category.Name = "Changed name";
  categoryRepository.Update(category);

  // To delete an entity:
  categoryRepository.Delete(category);

  // To read an entity by id:
  var result = categoryRepository
    .GetById(123);

  // To get all entities with includes, sorting and paging:
  result = categoryRepository
    .SortBy(p => p.Name)
    .Page(2, 50)
    .Include("Products"))
    .Find();

  // To read a filtered list using a where expression:
  result = categoryRepository.Find(c => c.Name == "Some name");

  // To read a filtered list using dynamic SQL:
  var parameters = new Dictionary<string, object>
  { 
    { "Id", 123 },
    { "Name", "MyName" }
  };
  result = categoryRepository.Find("EXEC FindCategory @Id, @Name", parameters);
  result = categoryRepository.Find("SELECT * FROM Category WHERE Id = @Id AND Name = @Name", parameters);
  ~~~~

### Wait, You Shouldn't Expose Queryable Collections from a Repository!
True in principle, because Linq to SQL implementations are incomplete and differ from one ORM framework to another. 
If you don't like Linq parameters, inherit from Repository and do your own implementation:

~~~~
  // Create inherited class:
  public class CategoryRepository : EntityFrameworkRepository<Category>
  {
    Category FindByName(string name, string sortBy, int pageNumber, int pageSize, string include)
    {
      return AsQueryable()
        .SortBy(sortBy)
        .Page(pageNumber, pageSize)
        .Include(include)
        .Find(c => c.Name == name);
    }
  }
~~~~

## How To Use RepositoryFramework.Api?
To GET https://jsonplaceholder.typicode.com/posts:
~~~~
  var apiRepository = new ApiRepository<Post>(
    new Configuration { AuthenticationType = AuthenticationType.Anonymous }, 
    "https://jsonplaceholder.typicode.com");

  var result = apiRepository.Find();
~~~~
The ApiRepository can recognize placeholders in the base path and set parameter values:

To GET https://SomeUrl.com/system/1/posts?userId=123:
~~~~
  var apiRepository = new ApiRepository<Post>(
    new Configuration { AuthenticationType = AuthenticationType.Anonymous }, 
    "https://SomeUrl.com/system/{systemId}/posts");

  var result = apiRepository
    .SetParameter("systemId", 1) // sets path parameter systemId to 1
    .SetParameter("userId", 123) // sets query parameter userId to 123
    .Find();
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
To specify GET parameters:
~~~~
  var result = apiRepository
    .SetParameter("UserId", 1)
    .Find();
~~~~

## How To Use RepositoryFramework.Dapper?
The Dapper generic repository does not support SQL injection since it not considered safe, see https://www.owasp.org/index.php/SQL_Injection. 
Queries using a method like this: Find($"where col = {formData}") is therefore intentionally opted out. To support queries, inherit the generic repository and pass forms data as parameters.

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

  // To create an entity:
  IDbConnection connection = CreateConnection();
  var categoryRepository = new DapperRepository<Category>(connection);
  var category = new Category
  {
    Name = Guid.NewGuid().ToString(),
    Description = Guid.NewGuid().ToString()
  };
  categoryRepository.Create(category);

  // To update an entity:
  category.Name = "Changed name";
  categoryRepository.Update(category);

  // To delete an entity:
  categoryRepository.Delete(category);

  // To get all entities with includes, sorting and paging:
  var result = categoryRepository
    .SortBy(p => p.Name)
    .Page(2, 50)
    .Include("Products"))
    .Find();

  // To read a filtered list using dynamic SQL:
  var parameters = new Dictionary<string, object>
  { 
    { "Id", 123 },
    { "Name", "MyName" }
  };
  result = categoryRepository.Find("EXEC FindCategory @Id, @Name", parameters);
  result = categoryRepository.Find("SELECT * FROM Category WHERE Id = @Id AND Name = @Name", parameters);

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
I agree. So I made StoredProcedureDapperRepository. This allows you to put all your SQL in the database and use stored procedures as an abstraction layer to SQL (and SQL dialects!).
In order to use StoredProcedureDapperRepository, you must create stored procedures using this naming convention:

* Create[Entity type name]
* Insert[Entity type name]
* Update[Entity type name]
* Delete[Entity type name]
* Find[Entity type name]

For example:

~~~~
CREATE PROCEDURE FindCategory
  @Name NVARCHAR(100) = NULL,
  @Description NVARCHAR(100) = NULL
AS
BEGIN
  SELECT * 
  FROM Category
  WHERE (Name = @Name OR @Name IS NULL)
    AND (Description = @Description OR @Description IS NULL)
END
~~~~

Having created the stored procedures, you can pass arguments to them like this:

~~~~
  new StoredProcedureDapperrepository<Category>()
    .SetParameter("Name", "Some name")
    .Find();
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

  // To create an entity:
  mongoRepository = new MongoRepository<TestDocument>(database);
  var doc = new TestDocument
  {
    StringTest = "A",
    IntTest = 1,
  };
  mongoRepository.Create(doc);

  // To read an entity by:
  var doc = categoryRepository.GetById(key);

  // To update an entity:
  doc.StringTest = "B";
  mongoRepository.Update(doc);

  // To delete an entity:
  mongoRepository.Delete(doc);

  // To read a list of entities with includes, sorting and paging:
  var result = categoryRepository
    .SortBy(doc => doc.IntTest)
    .Page(1, 50)
    .Find();

  // To filter using a where expression:
  var result = categoryRepository
    .Find(doc => doc.IntTest == 1);

  // To filter using a BSON filter definition:
  var result = categoryRepository
    .Find("{ IntTest: 1 }");
  ~~~~