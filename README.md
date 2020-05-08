See the big picture of **implementations on microservices with .net tools** on real-world **e-commerce microservices** project;

![aspnetrun-microservices](https://user-images.githubusercontent.com/1147445/79753821-34b93800-831f-11ea-86fc-617654557084.png)

There is a couple of microservices which implemented **e-commerce** modules over **Product, Basket** and **Ordering** microservices with **NoSQL (MongoDB, Redis)** and **Relational databases (Sql Server)** with communicating over **RabbitMQ Event Driven Communication** and using **Ocelot API Gateway**.

## Whats Including In This Repository
We have implemented below **features over the run-aspnetcore-microservices repository**.

#### Catalog microservice which includes; 
* ASP.NET Core Web API application 
* REST API principles, CRUD operations 
* Mongo DB NoSQL database connection on docker
* N-Layer implementation with Repository Pattern
* Swagger Open API implementation
* Dockerfile implementation

#### Basket microservice which includes;
* ASP.NET Core Web API application 
* REST API principles, CRUD operations 
* Redis database connection on docker
* Redis connection implementation
* RabbitMQ trigger event queue when checkout cart
* Swagger Open API implementation
* Dockerfile implementation

#### RabbitMQ messaging library which includes;
* Base EventBus implementation and add references Microservices

#### Ordering microservice which includes; (over the catalog specs)
* ASP.NET Core Web API application 
* Entity Framework Core on SQL Server docker
* REST API principles, CRUD operations 
* Consuming RabbitMQ messages
* Clean Architecture implementation with CQRS Pattern
* Event Sourcing
* Implementation of MediatR, Autofac, FluentValidator, AutoMapper
* Swagger Open API implementation
* Dockerfile implementation

#### API Gateway Ocelot microservice which includes;
* Routing to inside microservices
* Dockerization api-gateway

#### WebUI ShoppingApp microservice which includes;
* Asp.net Core Web Application with Razor template
* Call Ocelot APIs with HttpClientFactory
* Aspnet core razor tools - View Components, partial Views, Tag Helpers, Model Bindings and Validations, Razor Sections etc.. 

#### Docker Compose establishment with all microservices on docker;
* Dockerization of microservices
* Dockerization of database
* Override Environment variables

## Run The Project
You will need the following tools:

* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
* [.Net Core 3.1 or later](https://dotnet.microsoft.com/download/dotnet-core/3.1)
* [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Installing
Follow these steps to get your development environment set up:
1. Clone the repository
2. At the root directory which include **docker-compose.yml** files, run below command:
```csharp
docker-compose -f docker-compose.yml -f docker-compose.override.yml up –build
```
3. Wait for docker compose all microservices. That’s it!

4. You can **launch microservices** as below urls:
* **RabbitMQ -> http://localhost:15672/**
* **Catalog API -> http://localhost:8000/swagger/index.html**
* **Basket API -> http://localhost:8001/swagger/index.html**
* **Order API -> http://localhost:8002/swagger/index.html**
* **API Gateway -> http://localhost:7000/Order?username=swn**
* **Web UI -> http://localhost:8003/**

5. Launch http://localhost:8003/ in your browser to view the Web UI. You can use Web project in order to **call microservices over API Gateway**. When you **checkout the basket** you can follow **queue record on RabbitMQ dashboard**.

![mainscreen2](https://user-images.githubusercontent.com/1147445/81381837-08226000-9116-11ea-9489-82645b8dbfc4.png)

## The Book - Microservices Architecture and Step by Step Implementation on .NET

You can find **Microservices Architecture and Step by Step Implementation on .NET book** which **step by step developing** this repository with extensive explanations and details. This book is the **best path to leverage your .NET skills** in every aspect from begginer to senior level you can benefit to ramp-up faster on **Enterprice Application Development practices** and easy to **Onboarding to Full Stack .Net Core Developer jobs**. 
[Download Microservices Architecture and Step by Step Implementation on .NET Book](https://aspnetrun.azurewebsites.net/Microservices)

![aspnetrun_microservices3](https://user-images.githubusercontent.com/1147445/81383140-31dc8680-9118-11ea-992a-3ad8abc62314.png)

[Download Microservices Architecture and Step by Step Implementation on .NET Book](https://aspnetrun.azurewebsites.net/Microservices)

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




