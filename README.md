
![Screenshot_6](https://user-images.githubusercontent.com/1147445/85838002-907dc280-b7a1-11ea-8219-f84e3af8ba52.png)

**UDEMY COURSE WITH DISCOUNTED - Step by Step Development of this repository -> https://www.udemy.com/course/microservices-architecture-and-implementation-on-dotnet/?couponCode=C624710F8B411FE7A4FB**

**NOTE :** For those who comes from udemy, I have just updated the repository so udemy course will be updated soon. Since that time the course repository moved here ->
https://github.com/mehmetozkaya/MicroservicesApp

See the overall picture of **implementations on microservices with .net tools** on real-world **e-commerce microservices** project;

![aspnetrun-microservices](https://user-images.githubusercontent.com/1147445/79753821-34b93800-831f-11ea-86fc-617654557084.png)

There is a couple of microservices which implemented **e-commerce** modules over **Product, Basket, Discount** and **Ordering** microservices with **NoSQL (MongoDB, Redis)** and **Relational databases (PostgreSQL, Sql Server)** with communicating over **RabbitMQ Event Driven Communication** and using **Ocelot API Gateway**.

## Whats Including In This Repository
We have implemented below **features over the run-aspnetcore-microservices repository**.

#### Catalog microservice which includes; 
* ASP.NET Core Web API application 
* REST API principles, CRUD operations
* **MongoDB database** connection and containerization
* Repository Pattern Implementation
* Swagger Open API implementation	

#### Basket microservice which includes;
* ASP.NET Web API application
* REST API principles, CRUD operations
* **Redis database** connection and containerization
* Consume Discount **Grpc Service** for inter-service sync communication to calculate product final price
* Publish BasketCheckout Queue with using **MassTransit and RabbitMQ**
  
#### Discount microservice which includes;
* ASP.NET **Grpc Server** application
* Build a Highly Performant **inter-service gRPC Communication** with Basket Microservice
* Exposing Grpc Services with creating **Protobuf messages**
* Using **Dapper for micro-orm implementation** to simplify data access and ensure high performance
* **PostgreSQL database** connection and containerization

#### Microservices Communication
* Sync inter-service **gRPC Communication**
* Async Microservices Communication with **RabbitMQ Message-Broker Service**
* Using **RabbitMQ Publish/Subscribe Topic** Exchange Model
* Using **MassTransit** for abstraction over RabbitMQ Message-Broker system
* Publishing BasketCheckout event queue from Basket microservices and Subscribing this event from Ordering microservices	
* Create **RabbitMQ EventBus.Messages library** and add references Microservices

#### Ordering Microservice
* Implementing **DDD, CQRS, and Clean Architecture** with using Best Practices
* Developing **CQRS with using MediatR, FluentValidation and AutoMapper packages**
* Consuming **RabbitMQ** BasketCheckout event queue with using **MassTransit-RabbitMQ** Configuration
* **SqlServer database** connection and containerization
* Using **Entity Framework Core ORM** and auto migrate to SqlServer when application startup
	
#### API Gateway Ocelot Microservice
* Implement **API Gateways with Ocelot**
* Sample microservices/containers to reroute through the API Gateways
* Run multiple different **API Gateway/BFF** container types	
* The Gateway aggregation pattern in Shopping.Aggregator

#### WebUI ShoppingApp Microservice
* ASP.NET Core Web Application with Bootstrap 4 and Razor template
* Call **Ocelot APIs with HttpClientFactory** and **Polly**

#### Microservices Cross-Cutting Implementations
* Implementing **Centralized Distributed Logging with Elastic Stack (ELK); Elasticsearch, Logstash, Kibana and SeriLog** for Microservices
* Use the **HealthChecks** feature in back-end ASP.NET microservices
* Using **Watchdog** in separate service that can watch health and load across services, and report health about the microservices by querying with the HealthChecks

#### Microservices Resilience Implementations
* Making Microservices more **resilient Use IHttpClientFactory** to implement resilient HTTP requests
* Implement **Retry and Circuit Breaker patterns** with exponential backoff with IHttpClientFactory and **Polly policies**

#### Ancillary Containers
* Use **Portainer** for Container lightweight management UI which allows you to easily manage your different Docker environments
* **pgAdmin PostgreSQL Tools** feature rich Open Source administration and development platform for PostgreSQL

#### Docker Compose establishment with all microservices on docker;
* Containerization of microservices
* Containerization of databases
* Override Environment variables

**Quick DEMO on Youtube -> https://www.youtube.com/watch?v=p6lVqDNUYaY**

**Check Explanation of this Microservices Repository on Medium -> https://medium.com/aspnetrun/microservices-architecture-on-net-3b4865eea03f**

## Run The Project
You will need the following tools:

* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
* [.Net Core 5 or later](https://dotnet.microsoft.com/download/dotnet-core/5)
* [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Installing
Follow these steps to get your development environment set up: (Before Run Start the Docker Desktop)
1. Clone the repository
2. Once Docker for Windows is installed, go to the **Settings > Advanced option**, from the Docker icon in the system tray, to configure the minimum amount of memory and CPU like so:
* **Memory: 4 GB**
* CPU: 2
3. At the root directory which include **docker-compose.yml** files, run below command:
```csharp
docker-compose -f docker-compose.yml -f docker-compose.override.yml up –d
```
3. Wait for docker compose all microservices. That’s it! (some microservices need extra time to work so please wait if not worked in first shut)

4. You can **launch microservices** as below urls:

* **Catalog API -> http://host.docker.internal:8000/swagger/index.html**
* **Basket API -> http://host.docker.internal:8001/swagger/index.html**
* **Discount API -> http://host.docker.internal:8002/swagger/index.html**
* **Ordering API -> http://host.docker.internal:8004/swagger/index.html**
* **Shopping.Aggregator -> http://host.docker.internal:8005/swagger/index.html**
* **API Gateway -> http://host.docker.internal:8010/Catalog**
* **Rabbit Management Dashboard -> http://host.docker.internal:15672**   -- guest/guest
* **Portainer -> http://host.docker.internal:9000**   -- admin/admin1234
* **Elasticsearch -> http://host.docker.internal:9200**
* **Kibana -> http://host.docker.internal:5601**

* **Web Status -> http://host.docker.internal:8007**
* **Web UI -> http://host.docker.internal:8006**

5. Launch http://host.docker.internal:8007 in your browser to view the Web Status. Make sure that every microservices are healthy.
6. Launch http://host.docker.internal:8006 in your browser to view the Web UI. You can use Web project in order to **call microservices over API Gateway**. When you **checkout the basket** you can follow **queue record on RabbitMQ dashboard**.

![mainscreen2](https://user-images.githubusercontent.com/1147445/81381837-08226000-9116-11ea-9489-82645b8dbfc4.png)

>Note: If you are running this application in macOS then use `docker.for.mac.localhost` as DNS name in `.env` file and the above URLs instead of `host.docker.internal`.

## The Book - Microservices Architecture and Step by Step Implementation on .NET

You can find **Microservices Architecture and Step by Step Implementation on .NET book** which **step by step developing** this repository with extensive explanations and details. This book is the **best path to leverage your .NET skills** in every aspect from beginner to senior level you can benefit to ramp-up faster on **Enterprise Application Development practices** and easy to **Onboarding to Full Stack .Net Core Developer jobs**. 

The book also includes more practical information and I **update it regularly** and send it again with new versions.
So the idea is once you buy a book, I take this as **supporting me** and **join them in my privilege group** for sharing **next outputs**.
For example I am planning to add **IdentityServer4 implementation**, firstly I added it into a book and sent it to you.

**[Download Microservices Architecture and Step by Step Implementation on .NET Book](https://aspnetrun.azurewebsites.net/Microservices)**

![aspnetrun_microservices3](https://user-images.githubusercontent.com/1147445/81383140-31dc8680-9118-11ea-992a-3ad8abc62314.png)

**[Download Microservices Architecture and Step by Step Implementation on .NET Book](https://aspnetrun.azurewebsites.net/Microservices)**

## Upcomming Features

* Authentication with **IdentityServer4**
* Deployment with **AKS**
* Devops with **Azure Devops**

# What is AspnetRun ? 
The best path to **leverage your ASP.NET Core** skills. Onboarding to **Full Stack .Net Core Developer** jobs. Boilerplate for **ASP.NET Core reference application** with **Entity Framework Core**, demonstrating a layered application architecture with DDD best practices. Implements NLayer **Hexagonal architecture** (Core, Application, Infrastructure and Presentation Layers) and **Domain Driven Design** (Entities, Repositories, Domain/Application Services, DTO's...) 
and aimed to be a **Clean Architecture**, with applying **SOLID principles** in order to use for a project template. 
Also implements **best practices** like **loosely-coupled, dependency-inverted** architecture and using **design patterns** such as **Dependency Injection**, logging, validation, exception handling, localization and so on.

You can check full repository documentations and step by step development of **[100+ page e-book PDF](https://aspnetrun.azurewebsites.net)** from here - **https://aspnetrun.azurewebsites.net**. Also detail introduction of book and project structure exists on **[medium aspnetrun page](https://medium.com/aspnetrun)**. You can follow **aspnetrun repositories** for building **step by step** asp.net core **web development skills**.

# AspnetRun Repositories
Here you can find all of the **aspnetrun repositories from easy to difficult**, Also this list can be track a **learning path** of asp.net core respectively;
* **[run-aspnetcore-basics](https://github.com/aspnetrun/run-aspnetcore-basics)** - Building fastest ASP.NET Core Default Web Application template. This solution **only one solution one project** for **idea generation** with Asp.Net Core. 
* **[run-aspnetcore](https://github.com/aspnetrun/run-aspnetcore)** - Building ASP.NET Core Web Application with Entity Framework.Core and apply **Clean Architecture** with DDD best practices.
* **[run-aspnetcore-cqrs](https://github.com/aspnetrun/run-aspnetcore-cqrs)** - Building Single-Page Web Applications(SPA) using ASP.NET Core & EF.Core, Web API Project and implement **CQRS Design Pattern**.
* **[run-aspnetcore-microservices](https://github.com/aspnetrun/run-aspnetcore-microservices)** - Building **Microservices** on .Net platforms which used **Asp.Net Web API, Docker, RabbitMQ, Ocelot API Gateway, MongoDB, Redis, SqlServer, Entity Framework Core, CQRS and Clean Architecture** implementation.

## Give a Star! :star:
If you liked the project or if AspnetRun helped you, please **give a star**. And also please **fork** this repository and send us **pull-requests**. If you find any problem please open **issue**.

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

For information on upcoming features and fixes, take a look at the [product roadmap](https://github.com/aspnetrun/run-aspnetcore/projects).

## Deployment - AspnetRun Online

This organization page is deployed on Azure. See the project running on Azure in [here](https://aspnetrun.azurewebsites.net/).

## Pull-Request

Please fork this repository, and send me your findings with pull-requests. This is open-source repository so open to contributions.
Get your item from **missing features** [here from our project page](https://github.com/aspnetrun/run-aspnetcore-realworld/projects/1) and send us your pull requests.

## Authors

* **Mehmet Ozkaya** - *Initial work* - [mehmetozkaya](https://github.com/mehmetozkaya)

See also the list of [contributors](https://github.com/aspnetrun/run-core/contributors) who participated in this project. Check also [gihtub page of repository.](https://aspnetrun.github.io/run-aspnetcore-angular-realworld/)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
