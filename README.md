# Whats Including In This Repository

This is a complete example of implementation of a microservices-based architecture available for studying. This source code was forked and adapted from the repository [course](https://www.udemy.com/course/microservices-architecture-and-implementation-on-dotnet/?couponCode=AUGUST2021).

For more details about the application, please see this [link](https://github.com/aspnetrun/run-aspnetcore-microservices).

In this version of fork, you will find the following features:

# Features
## Deployment 

I created the deployment code using:

- Deployment to a local Kubernetes instance (**Minikube**), using **Helm charts**.
- Installation of **Istio** as a service Mesh solution [**To be done**].
- Using **Lens** for cluster management.

## Observability 

The following tools are availble using this deployment code:

- **Elasticsearch** and **Kibana**: Kibana is a data visualization and exploration tool used for log and time-series analytics and application monitoring. It uses Elasticsearch as search engine.
- **Healthchecks** implemented in each microservices using **AspNet Core health checks features**.


[**To be done**]
- **Kiali** : observability console for Istio with service mesh configuration and validation capabilities. It helps you understand the structure and health of your service mesh by monitoring traffic flow to infer the topology and report errors.
- **Jaeger** : open source software for tracing transactions between distributed services. It's used for monitoring and troubleshooting complex microservices environments.
- **Prometheus** and **Grafana**: Prometheus is free and an open-source event monitoring tool for containers or microservices. Grafana is a multi-platform visualization software available since 2014.

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

On your local machine you should now be able to reach the minikube registry by doing "port foward":

``kubectl port-forward --namespace kube-system service/registry 5000:80``

and run the curl below:

``curl http://localhost:5000/v2/_catalog``

## Install Helm

You will need to install Helm locally to be able to run the deployment script available in this repo.

Please see the official documentation [here](https://helm.sh/docs/intro/install/).

## Install Lens

Lens is a a Kubernetes IDE — open source project. Available for Linux, Mac, and Windows, Lens gives you a powerful interface and toolkit for managing, visualizing, and interacting with multiple Kubernetes clusters.

It will make your life easier, but you will also start forgeting all the kubectl commands you used to use.

The installation link is [here](https://www.mirantis.com/software/lens/).

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

## Accesing the application

At this point, the application should be available. You can access using one of the following options:

### Option (1): node port

Throught the node port **8089** configured on the file **deployment/k8s/helm/aspnetrunbasics/values.yaml**.

You can do a port forward to web application service exposed on this 8089 port: 

``kubectl port-forward --namespace default service/aspnetrun-aspnetrunbasics [YOUR_LOCAL_PORT]:8089``

### Option (2): using Lens

If you are using Lens, go to PODS, click on the **aspnetrunbasics** POD and click on **Ports** link:

![run_deploy](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/lens_aspnet.png)

### Option (3): cluster IP and service port

You can access using your **[cluster IP]:[Service Port]** exposed by the Web application service. To identify the cluster IP, you can you use:

``minikube Ip`` or ``kubectl cluster-info``

In my case, my cluster IP is 192.168.49.2:

![run_deploy](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/cluster_ip.png)

To identify the web application service port, you can you use:

``kubectl get svc | grep aspnetrunbasics``

In my case, my service port is 31293:

![run_deploy](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/service_port.png)

And, using the browser:

![run_deploy](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/browse_app.png)

## Accessing the application Web Status

You can follow the same options (1 and 2) explained above, but accessing the **webstatus** POD. The option (3) is not available, because this POD is not available outside the cluster.

![run_deploy](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/webstatus.png)

![run_deploy](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/webstatusbrowser.png)

## Accessing the APIs using Swagger

The microservices APIs are only available within the cluster. You can also use port-forward or access via LENS (/swagger/).

* catalog
* basket
* discount
* ordering

![run_deploy](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/swagger.png)


## Accessing Kibana (Elasticsearch)

The Kibana is only accessible within the cluster. You can also use port-forward or access via LENS. In the first access, you will need to configure the elasticsearch Index to be able to see the application logs. The configuration is beyond the scope of this documentation, but all the microservices are configure to send logs to the Elasticsearch container also running in the cluster.

![run_deploy](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/kibana_lens.png)

![run_deploy](https://github.com/felipecembranelli/run-aspnetcore-microservices/blob/PR_K8S/doc/kibana.png)
