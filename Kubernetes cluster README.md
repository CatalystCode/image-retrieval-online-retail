# Create a Kubernetes cluster with GPU VMs to train the ML model #

It's possible to deploy a ML model in a Kubernetes cluster using Azure Machine Learning ([here](https://docs.microsoft.com/en-us/azure/machine-learning/preview/deployment-setup-configuration)'s a step-by-step guide to set up the environment, create the Model Management account, and deploy the model locally and in a cluster), but at the moment of this writing the resulting cluster does not have GPU drivers installed. It means that it's not possible to use the power of GPUs to train the model. 

If GPUs are required, here's how to create a Kubernetes cluster with the proper drivers installed. 

First of all, we need to login to your Azure account and set the subscription you want to use 

    az login
    az account set --subscription <your-subscription>

Then, we can create the resource group for our cluster
    
    az group create --name <your-resource-group> --location <your-location>

Be careful to choose a region where [N-series VMs are available](https://azure.microsoft.com/en-us/regions/services/) and where also [ACS v2 is available](https://github.com/Azure/AKS/blob/master/preview_regions.md). Now we can create the cluster


    az acs create --orchestrator-type kubernetes --name <your-acs-name> --resource-group <your-resource-group> --generate-ssh-keys --location <your-location> --agent-vm-size Standard_NC6

*Please refer to the [official documentation]( https://docs.microsoft.com/en-us/cli/azure/acs?view=azure-cli-latest#az_acs_create) for more info and different options.*

Now we can get the cluster credentials and copy the kubeconfig file locally

    az acs kubernetes get-credentials --name <your-acs-name> --resource-group <your-resource-group>

The deployment of the cluster will take a while, and the installation of the drivers will take about 12 minutes longer. Let's take a look at our cluster:

    kubectl get nodes

![](img/screenshot1.jpg)

So if we check one of the nodes, we can see it has GPUs

    kubectl describe node <node-name>

![](img/screenshot2.png)

We can also verify if the GPUs are working properly downloading and running [this job](https://gist.github.com/wbuchwalter/c69ebba322781e8882f424e52833418c) (thanks to William Buchwalter!). 
    
    kubectl create -f nvidia-smi.yaml
    kubectl get pods
    
Copy the name of your pod from the ouput, so that you can check the logs and see that the GPUs are working properly.

    kubectl logs <pod-name>
    
![](img/screenshot3.png)

Now we can create the docker image with the model and the job to deploy it in the cluster using the GPUs. 
Basically, in order to use the GPUs, you need to expose the drivers from the host to the container and to specify how many of them we need (in the job template). The file should look like this one

![](img/screenshot4.jpg)

To have more details on how to do that, you can follow [these guidelines](https://github.com/Azure/acs-engine/blob/master/docs/kubernetes/gpu.md). 

Additional documentation:

- [kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/get-started-with-azure-cli?view=azure-cli-latest) 




