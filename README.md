# RepositoryFramework
A .Net framework for accessing data using the Repository pattern.
I used the code and advice given by Jonas Gauffin in this article: https://dzone.com/articles/repository-pattern-done-right, and elaborated on that.

## What is the "Repository Pattern" ?
I will refer you to this excellent article instead of venturing an explanation myself: https://martinfowler.com/eaaCatalog/repository.html!

## What is the "Repository Framework" ?
A collection of generic interfaces and utility classes that abstracts concrete implementations of repositories.
A concrete impementation using Entity Frameworkk Core can be found in the project RepositoryFramework.EntityFramework. 

## Why Should I Use This Repository Framework ?
You should't necessarily. Every tool has its purpose. 
If you are writing a simple application with a limited functional scope, you should use your data access framework directly. 
Don't bother setting up repositories unless you really need them. 
If you are building Microservices as part of a larger enterprise scale solution, streamlining your data access code, through the use of the Repository Framework, might turn out to be a good investment;
Simply because the code base will be easier to read and navigate.

## I See a lot of Interfaces, Why Not Just IRepository ?
Well, I decided that the Repository Framework should support these scenarios, because that was what I needed in my current project:
* A repository that supports the classical CRUD operations: Create, Read, Update and Delete.
* A repository that only supports a subset of CRUD, for example a read-only repository.
* A repository that supports Layered Executrion Trees (LET) or Linq to SQL, such as Entity Framework.
* A repository that does not support LET, such as a repository that calls a legacy data access framework or a downstream API.
* A repository that supports data paging and sorting.

## What Does IUnitOfWork Do ?
Gives you the opportunity to control when changes are committed to the data storage by calling the method SaveChanges().

## Interfaces for Repositories that Support Linq to SQL
These interfaces are typically used by repositories that support Linq to SQL, and are used in the Entity Framework Core implementation in RepositoryFramework.EntityFramework:
* IUnitOfWork
* IRepository
* IGetQueryable
* IFindQueryable

## Interfaces for Repositories with No Support for Linq to SQL
These interfaces are typically used by repositories with no support for Linq to SQL:
* IRepository
* IGet
* IFindFilter

## How Does the Repository Framework Interfaces Work ?
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

## Wait, You Shouldn't Expose Linq from a Repository!
True in principle, because Linq to SQL implementations are incomplete and differ from one ORM framework to another. 
But I need it! If you don't like Linq in the Find() and GetById() methods, use the non-Linq alternatives in IGet and IFindFilter:

~~~~
  public class CategoryFilter
  {
    public int Id { get; set; }
    public string Name { get; set; }
  }

...

  categoryRepository.GetById(new CategoryFilter { Id = 123 });

...

  categoryRepository.Find(new CategoryFilter { Name = "My category" });
~~~~

## Why Are RepositoryFramework and RepositoryFramework.EntityFramework in Different Packages ?
Because I plan to make (or convince someone else to make) more implementation packages such as:
* RepositoryFramework.Api (a generic REST API client repository)
* RepositoryFramework.Dapper
* RepositoryFramework.NHibernate (well, if and when NHibernate is ported to .Net Core, which it is currently not)