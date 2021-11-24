## Whats Including In This Repository

This is a complete example of implementation of a microservices-based architecture made available for study. This source code was forked and adapted from the Repository [course](https://www.udemy.com/course/microservices-architecture-and-implementation-on-dotnet/?couponCode=AUGUST2021).

For more details about the application, please see this [link](https://github.com/aspnetrun/run-aspnetcore-microservices).

In this version of fork, you will find the following features:

## Deployment 

I created the deployment code using:

- Deployment to a local Kubernetes instance (**Minikube**), using **Helm charts**.
- Installation of **Istio** as a service Mesh solution.
- Using **Lens** for cluster management.

## Observability 

The following tools are availble using this deployment code:

- **Kiali** : observability console for Istio with service mesh configuration and validation capabilities. It helps you understand the structure and health of your service mesh by monitoring traffic flow to infer the topology and report errors.
- **Jaeger** : open source software for tracing transactions between distributed services. It's used for monitoring and troubleshooting complex microservices environments.
- **Prometheus** and **Grafana**: Prometheus is free and an open-source event monitoring tool for containers or microservices. Grafana is a multi-platform visualization software available since 2014.
- **Elasticsearch** and **Kibana**: Kibana is a data visualization and exploration tool used for log and time-series analytics and application monitoring. It uses Elasticsearch as search engine.
- **Healthchecks** implemented in each microservices using **AspNet Core health checks features**.

## Scaling (To be done)
- **HPA**
- **Keda**

# Run the application using Docker

In order to run the application on the local machine, follow the [original repository documentation](https://github.com/aspnetrun/run-aspnetcore-microservices).

# Run the application using Local Kubernetes
## Create your local Kubernetes Cluster

Minikube is local Kubernetes, focusing on making it easy to learn and develop for Kubernetes. Follow the installation documentation below:

[Minikube offical documentaion](https://minikube.sigs.k8s.io/docs/start/)

After the installation is finished with success, you should be able to see the Minikube pods running like this:

![minikube](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/minikube_1.png)

## Create local Registry

A container registry is a repository, or collection of repositories, used to store container images for Kubernetes, DevOps, and container-based application development.

I decided to create my own local container registry, but you can use whatever you want to host your container images. 

I followed this [documentation](https://minikube.sigs.k8s.io/docs/handbook/registry/) to create my Registry using Minikube.

Once you have the addon enabled, you should be able to connect to it. When enabled, the registry addon exposes its **port 5000** on the minikube’s virtual machine.

On your local machine you should now be able to reach the minikube registry by doing "portfoward":

``kubectl port-forward --namespace kube-system service/registry 5000:80``

and run the curl below:

``curl http://localhost:5000/v2/_catalog``

## Install Istio

## Install Lens

## Install Helm

## Application Deployment

Assuming that you already have your container images built, you should now push them to the registry. To do that, you should (example):

``kubectl port-forward --namespace kube-system service/registry 5000:80``

Tag the image:

``docker tag ocelotapigw localhost:5000/ocelotapigw``

Push to the registry:

``docker push localhost:5000/ocelotapigw``

Verify if the images are available on the registry:
![registry](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/registry_images.png)


After that, you are ready to start the application deployment into the Kubernetes cluster. The next step is to run the script below, that will create the pods, services and other K8S resources needed to run the application.

### Installing the application using helm charts

You will need Powershell installed. If you are using Linux like me, take a look on this [link](https://adamtheautomator.com/powershell-linux/).

Go to the folder /run-aspnetcore-microservices/deployment/k8s/helm and run:

``pwsh``

![pwsh](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/pwsh.png)

Run the script below:

``./deploy-all.ps1``

![run_deploy](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/run_deploy.png)

You should see the pods running after some seconds:

![pods_running](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/pods_running.png)



## Using Ingress Controller

To be done.






















*Repository forked from https://www.udemy.com/course/microservices-architecture-and-implementation-on-dotnet/?couponCode=AUGUST2021

*I have created a "lite version"  of Ordering component, removing CQRS implementation.



See the overall picture of **implementations on microservices with .net tools** on real-world **e-commerce microservices** project;

![microservices_remastered](https://user-images.githubusercontent.com/1147445/110304529-c5b70180-800c-11eb-832b-a2751b5bda76.png)

There is a couple of microservices which implemented **e-commerce** modules over **Catalog, Basket, Discount** and **Ordering** microservices with **NoSQL (MongoDB, Redis)** and **Relational databases (PostgreSQL, Sql Server)** with communicating over **RabbitMQ Event Driven Communication** and using **Ocelot API Gateway**.

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
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

>Note: If you get connection timeout error Docker for Mac please [Turn Off Docker's "Experimental Features".](https://github.com/aspnetrun/run-aspnetcore-microservices/issues/33)

4. Wait for docker compose all microservices. That’s it! (some microservices need extra time to work so please wait if not worked in first shut)

5. You can **launch microservices** as below urls:

* **Catalog API -> http://host.docker.internal:8000/swagger/index.html**
* **Basket API -> http://host.docker.internal:8001/swagger/index.html**
* **Discount API -> http://host.docker.internal:8002/swagger/index.html**
* **Ordering API -> http://host.docker.internal:8004/swagger/index.html**
* **Shopping.Aggregator -> http://host.docker.internal:8005/swagger/index.html**
* **API Gateway -> http://host.docker.internal:8010/Catalog**
* **Rabbit Management Dashboard -> http://host.docker.internal:15672**   -- guest/guest
* **Portainer -> http://host.docker.internal:9000**   -- admin/admin1234
* **pgAdmin PostgreSQL -> http://host.docker.internal:5050**   -- admin@aspnetrun.com/admin1234
* **Elasticsearch -> http://host.docker.internal:9200**
* **Kibana -> http://host.docker.internal:5601**

* **Web Status -> http://host.docker.internal:8007**
* **Web UI -> http://host.docker.internal:8006**

5. Launch http://host.docker.internal:8007 in your browser to view the Web Status. Make sure that every microservices are healthy.
6. Launch http://host.docker.internal:8006 in your browser to view the Web UI. You can use Web project in order to **call microservices over API Gateway**. When you **checkout the basket** you can follow **queue record on RabbitMQ dashboard**.

![mainscreen2](https://user-images.githubusercontent.com/1147445/81381837-08226000-9116-11ea-9489-82645b8dbfc4.png)

>Note: If you are running this application in macOS then use `docker.for.mac.localhost` as DNS name in `.env` file and the above URLs instead of `host.docker.internal`.

## Authors

* **Mehmet Ozkaya** - *Initial work* - [mehmetozkaya](https://github.com/mehmetozkaya)

See also the list of [contributors](https://github.com/aspnetrun/run-core/contributors) who participated in this project. Check also [gihtub page of repository.](https://aspnetrun.github.io/run-aspnetcore-angular-realworld/)

