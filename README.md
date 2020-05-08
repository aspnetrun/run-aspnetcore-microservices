# run-aspnetcore-microservices

Here is **implementation of microservices with .net tools** on real-world **e-commerce microservices** project;

![aspnetrun-microservices](https://user-images.githubusercontent.com/1147445/79753821-34b93800-831f-11ea-86fc-617654557084.png)

There is a couple of microservices which implemented **e-commerce** modules over **Product, Basket** and **Order** microservices with **NoSQL (MongoDB, Redis)** and **Relational databases (Sql Server)** with communicating over **RabbitMQ Event Driven Communication** and using **Ocelot API Gateway**.

## Whats Including In This Repository
We have implemented below **features over the run-aspnetcore-microservices repository**.

#### Create Catalog microservice which includes; 
* ASP.NET Core Web API application 
* REST API principles, CRUD operations 
* Mongo DB NoSQL database connection on docker
* N-Layer implementation with repository pattern
* Swagger Open API implementation
* Dockerfile implementation

#### Create Basket microservice which includes;
* ASP.NET Core Web API application 
* REST API principles, CRUD operations 
* Redis database connection on docker
* Redis connection implementation
* RabbitMQ trigger event queue when checkout cart
* Swagger Open API implementation
* Dockerfile implementation

#### Create RabbitMQ messaging library which includes;
* Base EventBus implementation and add references Microservices

#### Create Ordering microservice which includes; (over the catalog specs)
* ASP.NET Core Web API application 
* Entity Framework Core on SQL Server docker
* REST API principles, CRUD operations 
* Consuming RabbitMQ messages
* Clean Architecture implementation with CQRS Pattern
* Event Sourcing
* Implementation of MediatR, Autofac, FluentValidator, AutoMapper
* Swagger Open API implementation
* Dockerfile implementation

#### Create API Gateway Ocelot microservice which includes;
* Routing to inside microservices
* Dockerization api-gateway

#### Create WebUI ShoppingApp microservice which includes;
* Asp.net Core Web Application with Razor template
* Call Ocelot APIs with HttpClientFactory
* Aspnet core razor tools - View Components, partial Views, Tag Helpers, Model Bindings and Validations, Razor Sections etc.. 

#### Docker Compose establishment with all microservices on docker;
* Dockerization of microservices
* Dockerization of database
* Override Environment variables

## Give a Star! :star:
If you liked the project or if AspnetRun helped you, please **give a star**. And also please **fork** this repository and send us **pull-requests**. If you find any problem please open **issue**.

## Getting Started
Use these instructions to get the project up and running.

### Prerequisites
You will need the following tools:

* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
* [.Net Core 2.2 or later](https://dotnet.microsoft.com/download/dotnet-core/2.2)
* EF Core 2.2 or later

### Installing
Follow these steps to get your development environment set up:
1. Clone the repository
2. At the root directory, restore required packages by running:
```csharp
dotnet restore
```
3. Next, build the solution by running:
```csharp
dotnet build
```
4. Next, within the AspnetRun.Web directory, launch the back end by running:
```csharp
dotnet run
```
5. Launch http://localhost:5400/ in your browser to view the Web UI.

If you have **Visual Studio** after cloning Open solution with your IDE, AspnetRun.Web should be the start-up project. Directly run this project on Visual Studio with **F5 or Ctrl+F5**. You will see index page of project, you can navigate product and category pages and you can perform crud operations on your browser.

### Usage
After cloning or downloading the sample you should be able to run it using an In Memory database immediately. The default configuration of Entity Framework Database is **"InMemoryDatabase"**.
If you wish to use the project with a persistent database, you will need to run its Entity Framework Core **migrations** before you will be able to run the app, and update the ConfigureDatabases method in **Startup.cs** (see below).

```csharp
public void ConfigureDatabases(IServiceCollection services)
{
    // use in-memory database
    services.AddDbContext<AspnetRunContext>(c =>
        c.UseInMemoryDatabase("AspnetRunConnection")
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

    //// use real database
    //services.AddDbContext<AspnetRunContext>(c =>
    //    c.UseSqlServer(Configuration.GetConnectionString("AspnetRunConnection"))
    //    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
}
```

1. Ensure your connection strings in ```appsettings.json``` point to a local SQL Server instance.

2. Open a command prompt in the Web folder and execute the following commands:

```csharp
dotnet restore
dotnet ef database update -c AspnetRunContext -p ../AspnetRun.Infrastructure/AspnetRun.Infrastructure.csproj -s AspnetRun.Web.csproj
```
Or you can direct call ef commands from Visual Studio **Package Manager Console**. Open Package Manager Console, set default project to AspnetRun.Infrastructure and run below command;
```csharp
update-database
```
These commands will create aspnetrun database which include Product and Category table. You can see from **AspnetRunContext.cs**.
1. Run the application.
The first time you run the application, it will seed aspnetrun sql server database with a few data such that you should see products and categories.

If you modify-change or add new some of entities to Core project, you should run ef migrate commands in order to update your database as the same way but below commands;
```csharp
add migration YourCustomEntityChanges
update-database
```

## Layered Architecture
AspnetRun implements NLayer **Hexagonal architecture** (Core, Application, Infrastructure and Presentation Layers) and **Domain Driven Design** (Entities, Repositories, Domain/Application Services, DTO's...). Also implements and provides a good infrastructure to implement **best practices** such as Dependency Injection, logging, validation, exception handling, localization and so on.
Aimed to be a **Clean Architecture** also called **Onion Architecture**, with applying **SOLID principles** in order to use for a project template. Also implements and provides a good infrastructure to implement **best practices** like **loosely-coupled, dependency-inverted** architecture
The below image represents aspnetrun approach of development architecture of run repository series;

![DDD_png_pure](https://user-images.githubusercontent.com/1147445/54773098-e1efe700-4c19-11e9-9150-74f7e770de42.png)

### Structure of Project
Repository include layers divided by **4 project**;
* Core
    * Entities    
    * Interfaces
    * Specifications
    * ValueObjects
    * Exceptions
* Application    
    * Interfaces    
    * Services
    * Dtos
    * Mapper
    * Exceptions
* Infrastructure
    * Data
    * Repository
    * Services
    * Migrations
    * Logging
    * Exceptions
* Web
    * Interfaces
    * Services
    * Pages
    * ViewModels
    * Extensions
    * AutoMapper

### Core Layer
Development of Domain Logic with abstraction. Interfaces drives business requirements with light implementation. The Core project is the **center of the Clean Architecture** design, and all other project dependencies should point toward it. 

#### Entities
Includes Entity Framework Core Entities which creates sql table with **Entity Framework Core Code First Aproach**. Some Aggregate folders holds entity and aggregates.
You can see example of **code-first** Entity definition as below;

```csharp
public class Product : BaseEntity
{        
    public string ProductName { get; set; }
    public string QuantityPerUnit { get; set; }
    public decimal? UnitPrice { get; set; }
    public short? UnitsInStock { get; set; }
    public short? UnitsOnOrder { get; set; }
    public short? ReorderLevel { get; set; }
    public bool Discontinued { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public static Product Create(int productId, int categoryId, string name, decimal? unitPrice = null, short? unitsInStock = null, short? unitsOnOrder = null, short? reorderLevel = null, bool discontinued = false)
    {
        var product = new Product
        {
            Id = productId,
            CategoryId = categoryId,
            ProductName = name,
            UnitPrice = unitPrice,
            UnitsInStock = unitsInStock,
            UnitsOnOrder = unitsOnOrder,
            ReorderLevel = reorderLevel,
            Discontinued = discontinued
        };
        return product;
    }
}
```
Applying domain driven approach, Product class responsible to create Product instance. 

#### Interfaces
Abstraction of Repository - Domain repositories (IAsyncRepository - IProductRepository) - Specifications etc.. This interfaces include database operations without any application and ui responsibilities.

#### Specifications
This folder is implementation of **[specification pattern](https://en.wikipedia.org/wiki/Specification_pattern)**. Creates custom scripts with using **ISpecification** interface. Using BaseSpecification managing Criteria, Includes, OrderBy, Paging.
This specs runs when EF commands working with passing spec. This specs implemented SpecificationEvaluator.cs and creates query to AspnetRunRepository.cs in ApplySpecification method.This helps create custom queries.

### Infrastructure Layer
Implementation of Core interfaces in this project with **Entity Framework Core** and other dependencies.
Most of your application's dependence on external resources should be implemented in classes defined in the Infrastructure project. These classes must implement the interfaces defined in Core. If you have a very large project with many dependencies, it may make sense to have more than one Infrastructure project (eg Infrastructure.Data), but in most projects one Infrastructure project that contains folders works well.
This could be includes, for example, **e-mail providers, file access, web api clients**, etc. For now this repository only dependend sample data access and basic domain actions, by this way there will be no direct links to your Core or UI projects.

#### Data
Includes **Entity Framework Core Context** and tables in this folder. When new entity created, it should add to context and configure in context.
The Infrastructure project depends on Microsoft.**EntityFrameworkCore.SqlServer** and EF.Core related nuget packages, you can check nuget packages of Infrastructure layer. If you want to change your data access layer, it can easily be replaced with a lighter-weight ORM like Dapper. 

#### Migrations
EF add-migration classes.
#### Repository
EF Repository and Specification implementation. This class responsible to create queries, includes, where conditions etc..
#### Services
Custom services implementation, like email, cron jobs etc.

### Application Layer
Development of **Domain Logic with implementation**. Interfaces drives business requirements and implementations in this layer.
Application layer defines that user required actions in app services classes as below way;

```csharp
public interface IProductAppService
{
    Task<IEnumerable<ProductDto>> GetProductList();
    Task<ProductDto> GetProductById(int productId);
    Task<IEnumerable<ProductDto>> GetProductByName(string productName);
    Task<IEnumerable<ProductDto>> GetProductByCategory(int categoryId);
    Task<ProductDto> Create(ProductDto entityDto);
    Task Update(ProductDto entityDto);
    Task Delete(ProductDto entityDto);
}
```
Also implementation located same places in order to choose different implementation at runtime when DI bootstrapped.
```csharp
public class ProductAppService : IProductAppService
{
    private readonly IProductRepository _productRepository;
    private readonly IAppLogger<ProductAppService> _logger;

    public ProductAppService(IProductRepository productRepository, IAppLogger<ProductAppService> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<ProductDto>> GetProductList()
    {
        var productList = await _productRepository.GetProductListAsync();
        var mapped = ObjectMapper.Mapper.Map<IEnumerable<ProductDto>>(productList);
        return mapped;
    }
}
```
In this layer we can add validation , authorization, logging, exception handling etc. -- cross cutting activities should be handled in here.

### Web Layer
Development of UI Logic with implementation. Interfaces drives business requirements and implementations in this layer.
The application's main **starting point** is the ASP.NET Core web project. This is a classical console application, with a public static void Main method in Program.cs. It currently uses the default **ASP.NET Core project template** which based on **Razor Pages** templates. This includes appsettings.json file plus environment variables in order to stored configuration parameters, and is configured in Startup.cs.

Web layer defines that user required actions in page services classes as below way;
```csharp
public interface IProductPageService
{
    Task<IEnumerable<ProductViewModel>> GetProducts(string productName);
    Task<ProductViewModel> GetProductById(int productId);
    Task<IEnumerable<ProductViewModel>> GetProductByCategory(int categoryId);
    Task<IEnumerable<CategoryViewModel>> GetCategories();
    Task<ProductViewModel> CreateProduct(ProductViewModel productViewModel);
    Task UpdateProduct(ProductViewModel productViewModel);
    Task DeleteProduct(ProductViewModel productViewModel);
}
```
Also implementation located same places in order to choose different implementation at runtime when DI bootstrapped.
```csharp
public class ProductPageService : IProductPageService
{
    private readonly IProductAppService _productAppService;
    private readonly ICategoryAppService _categoryAppService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductPageService> _logger;

    public ProductPageService(IProductAppService productAppService, ICategoryAppService categoryAppService, IMapper mapper, ILogger<ProductPageService> logger)
    {
        _productAppService = productAppService ?? throw new ArgumentNullException(nameof(productAppService));
        _categoryAppService = categoryAppService ?? throw new ArgumentNullException(nameof(categoryAppService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<ProductViewModel>> GetProducts(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            var list = await _productAppService.GetProductList();
            var mapped = _mapper.Map<IEnumerable<ProductViewModel>>(list);
            return mapped;
        }

        var listByName = await _productAppService.GetProductByName(productName);
        var mappedByName = _mapper.Map<IEnumerable<ProductViewModel>>(listByName);
        return mappedByName;
    }
}
```
### Test Layer
For each layer, there is a test project which includes intended layer dependencies and mock classes. So that means Core-Application-Infrastructure and Web layer has their own test layer. By this way this test projects also divided by **unit, functional and integration tests** defined by in which layer it is implemented. 
Test projects using **xunit and Mock libraries**.  xunit, because that's what ASP.NET Core uses internally to test the product. Moq, because perform to create fake objects clearly and its very modular.


## Technologies
* .NET Core 2.2
* ASP.NET Core 2.2
* Entity Framework Core 2.2 
* .NET Core Native DI
* Razor Pages
* AutoMapper

## Architecture
* Clean Architecture
* Full architecture with responsibility separation of concerns
* SOLID and Clean Code
* Domain Driven Design (Layers and Domain Model Pattern)
* Unit of Work
* Repository and Generic Repository
* Multiple Page Web Application (MPA)
* Monolitic Deployment Architecture
* Specification Pattern

## Disclaimer

* This repository is not intended to be a definitive solution.
* This repository not implemented a lot of 3rd party packages, we are try to avoid the over engineering when building on best practices.
* Beware to use in production way.

## Contributing

Please read [Contributing.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.
We have a lot of **missing features** you can check [here from our project page](https://github.com/aspnetrun/run-aspnetcore-realworld/projects/1) and you can develop them. We are waiting for your pull requests.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/aspnetrun/run-core/tags). 

## Next Releases and RoapMap

For information on upcoming features and fixes, take a look at the [product roadmap](https://github.com/aspnetrun/run-core/projects).

## Deployment - AspnetRun Online

This project is deployed on Azure. See the project running on Azure in [here](aspnetrun.com).

## Pull-Request

Please fork this repository, and send me your findings with pull-requests. This is open-source repository so open to contributions.
Get your item from **missing features** [here from our project page](https://github.com/aspnetrun/run-aspnetcore-realworld/projects/1) and send us your pull requests.

## Authors

* **Mehmet Ozkaya** - *Initial work* - [mehmetozkaya](https://github.com/mehmetozkaya)

See also the list of [contributors](https://github.com/aspnetrun/run-core/contributors) who participated in this project. Check also [gihtub page of repository.](https://aspnetrun.github.io/run-aspnetcore-angular-realworld/)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details




