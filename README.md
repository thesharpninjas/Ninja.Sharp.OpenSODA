
OpenSODA - a .Net driver for Oracle SODA
========================================
Release Notes
-------------
First release has been created!

Packages
--------

| Package | NuGet Stable | 
| ------- | ------------ | 
| [OpenSODA](https://www.nuget.org/packages/Ninja.Sharp.OpenSODA/) | [![OpenSODA](https://img.shields.io/badge/nuget-v0.0.1-blue)](https://www.nuget.org/packages/OpenSODA/)
| [OpenSODA.Driver.Rest](https://www.nuget.org/packages/Ninja.Sharp.OpenSODA.Driver.Rest/) | [![OpenSODA](https://img.shields.io/badge/nuget-v0.0.1-blue)](https://www.nuget.org/packages/Ninja.Sharp.OpenSODA.Driver.Rest/) | 
| [OpenSODA.Driver.Sql.Native](https://www.nuget.org/packages/Ninja.Sharp.OpenSODA.Driver.Sql.Native/) | [![OpenSODA](https://img.shields.io/badge/nuget-v0.0.1-blue)](https://www.nuget.org/packages/Ninja.Sharp.OpenSODA.Driver.Sql.Native/) | 
| [OpenSODA.Driver.Sql.Qbe](https://www.nuget.org/packages/Ninja.Sharp.OpenSODA.Driver.Sql.Qbe/) | [![OpenSODA](https://img.shields.io/badge/nuget-v0.0.1-blue)](https://www.nuget.org/packages/Ninja.Sharp.OpenSODA.Driver.Sql.Qbe/) | 


Package Purposes:
* OpenSODA
  * The core library
* OpenSODA.Driver.Rest
  * Extension handlers for SODA Rest APIs
* OpenSODA.Driver.Sql.Native
  * Extension handlers for SODA using Oracle Sql providers
* OpenSODA.Driver.Sql.Qbe
  * Extension handlers for SODA using Oracle drivers

Features
--------
OpenSODA is a [NuGet library](https://www.nuget.org/packages/Ninja.Sharp.OpenSODA) that you can use to connect and manage entities provided by Oracle SODA (*Simple Oracle Document Access*).
The library provides simple and efficient methods for saving, retrieveing and filtering your entities, with support for both synchronous and
asynchronous data access.

It provides multiple integration to SODA instances:

- via **REST APIs**: you can use Oracle Rest APIs to query your Oracle SODA installation.
- via **QBE**: you can filter documents via query-by-example (QBE). This is the most efficient way to connect to SODA. However, it does not natively support serch via textcontains and search in document arrays
- via **SQL**: you can always query your SODA database through SQL instructions

## Configuration
OpenSODA can be configured manually or via appsettings.json. Configuration differs for Rest and Sql implementation.

If you're using Rest APIs, then

``` json
{
  "Soda": {
    "Host": "10.10.10.10",
    "Username": "Username",
    "Password": "Password",
    "Schema": "Schema",
    "IsSecured": true,
    "Port": 8080,
    "ServiceName": "ServiceName"
  }
}
```
otherwise, if you're using SQL (both native or QBE)
``` json
{
  "Soda": {
    "Host": "10.10.10.10",
    "Username": "Username",
    "Password": "Password",
    "Schema": "Schema",
    "Port": 1521,
    "ServiceName": "ServiceName"
  }
}
```

## Choose and add a provider
OpenSODA manages for you a simple implementation (*IDocumentDbProvider*) you can use to perform your queries.
To inject a different implementation, you just have to add to your services the desired one


``` csharp
services
   //.AddOracleSodaRestServices(configuration)
   //.AddOracleSodaSqlNativeServices(configuration)
   .AddOracleSodaSqlQbeServices(configuration)
   .BuildServiceProvider();
```

- **AddOracleSodaRestServices**: adds an implementation of *IDocumentDbProvider* that uses Rest APIs. You'll need the *OpenSODA.Driver.Rest* package to inject this one.
- **AddOracleSodaSqlNativeServices**: adds an implementation of *IDocumentDbProvider* that will perform queries in a SQL-like manner. You'll need *OpenSODA.Driver.Sql.Native* package to inject this one.
*we suggest adding a global index to your Oracle SODA database prior to using this implementation*
- **AddOracleSodaSqlQbeServices**: adds an implementation of *IDocumentDbProvider* that will perform queries using Oracle SODA *query-by-example* (QBE). You'll need the *OpenSODA.Driver.Sql.QBE* package to inject this one.

Whichever package you choose, the provided methods are the same, so feel free to change the provider and verify which one is the best for your use-case.

## Creating a collection
Before adding data to your database, you have to create a collection. A document *collection* contains *documents*. A SODA *collection* is analogous to an Oracle Database *table* or *view*.

To create a collection, you have two ways:
 - you can provide the collection name
``` csharp
  await sodaProvider.CreateCollectionIfNotExistsAsync("myCollection");
```
 - or you can annotate an item with *CollectionAttribute*. The name provided in the attribute will be the collection name
``` csharp
  [Collection("myCollection")]
  internal class TestObject
  {
      public string One { get; set; } = string.Empty;
      public string Two { get; set; } = string.Empty;
      public string Three { get; set; } = string.Empty;
  }
```
``` csharp
  await sodaProvider.CreateCollectionIfNotExistsAsync<TestObject>();
```
*We suggest you use the former method. Items you're going to persist must have the Collection attribute in order to be saved*.

## Insert an element
To add an element to a collection, first you need to annotate the class item with the Collection attribute
 ``` csharp
  [Collection("myCollection")]
  internal class TestObject
  {
      public string One { get; set; } = string.Empty;
      public string Two { get; set; } = string.Empty;
      public string Three { get; set; } = string.Empty;
  }
```
*If you do not provide a collection name, and you do not annotate your model, the library will use the class name as collection*.

Then, you can just
 ``` csharp
  
  private readonly IDocumentDbProvider sodaProvider = sodaProvider;

  public async Task DoStuff()
  {
    Item<TestObject> item = await sodaProvider.UpsertAsync(new TestObject { One = value });
  }
```
When you add an item, you will get your item back with some additional information added by SODA, such as the ID and the creation date.
The retrieve task will use the provided ID.

## Retrieve an element
``` csharp
  
  private readonly IDocumentDbProvider sodaProvider = sodaProvider;

  public async Task DoStuff()
  {
    Item<TestObject> item = await sodaProvider.UpsertAsync(new TestObject { One = value });
    item = await sodaProvider.RetrieveAsync<TestObject>(item.Id);
  }
```

## Retrieve data
``` csharp
  
  private readonly IDocumentDbProvider sodaProvider = sodaProvider;

  public async Task DoStuff()
  {
    var results = await sodaProvider.ListAsync<TestObject>(new Page
    {
        ItemsPerPage = 10,
        PageNumber = 1,
        Ordering = Ordering.Descending,
        OrderingPath = nameof(TestObject.One),
    });
  }
```
## Delete data
``` csharp
  
  private readonly IDocumentDbProvider sodaProvider = sodaProvider;

  public async Task DoStuff()
  {
    Item<TestObject> item = await sodaProvider.UpsertAsync(new TestObject { One = value });
    await sodaProvider.DeleteAsync<TestObject>(item.Id);
  }
```

## Filtering
To query your database, we provide a constructible query system using operands and data types.
``` csharp
  
  private readonly IDocumentDbProvider sodaProvider = sodaProvider;

  public async Task DoStuff()
  {
    Query query = new();
    query.With(new And()
      .With(new OString(nameof(TestObject.One), item.Value.One))
      .With(new OString(nameof(TestObject.Two), item.Value.Two))
    );

    var results = await sodaProvider.FilterAsync<TestObject>(query);   
  }
```

The current available operands are
 - and 
 - not 
 - or

The current available primitives are
 - datetime 
 - int
 - string
 - upperString (*Oracle SODA is case sensitive*)

 More operands will be released in the future.

 ## Limitations
 Some queries could be highly inefficient: often SODA will perform a full table scan to retrieve some data.
 We suggest to manually add a global index if you're using Sql.Native provider, and to add partitioning and cluster indexes if you're using Sql.QBE.

 ## Contributing
Thank you for considering to help out with the source code!
If you'd like to contribute, please fork, fix, commit and send a pull request for the maintainers to review and merge into the main code base.
 
**Getting started with Git and GitHub**
 
* [Setting up Git for Windows and connecting to GitHub](http://help.github.com/win-set-up-git/)
* [Forking a GitHub repository](http://help.github.com/fork-a-repo/)
* [The simple guide to GIT guide](http://rogerdudler.github.com/git-guide/)
* [Open an issue](https://github.com/thesharpninjas/Ninja.Sharp.OpenSODA/issues) if you encounter a bug or have a suggestion for improvements/features
****