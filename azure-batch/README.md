# Azure Batch instrumentation

The code you can find here follows closely the tutorial which can be found here: https://docs.microsoft.com/en-us/azure/batch/batch-python-tutorial

The main changes are the following:

- `python_gist_client.py`: if you need to use custom VM images, we have created a function called `create_pool_with_custom_image`, provide it with your custom image id (see code for details). 
Note: We initially used the function `create_pool_with_custom_image` as we wanted a VM with pyleargist already installed. This code though is not used in the final solution and we ended up successfully leveraging the default Azure images. We are nonetheless giving some details here because using custom VM images can give as a benefit a lower node startup time (as there is no start-up script to be run). Furthermore at the time of writing we did not find any documentation supporting the programmatic creation in Python of Azure Batch pools based on custom VM image. For details on how to create a custom VM image follow these instustructions https://docs.microsoft.com/en-us/azure/virtual-machines/linux/capture-image. To make this code work you will also need to create an App identity and give it permissions to the resource group where you created your custom VM Image. To create an Azure App identity follow these instructions https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-create-service-principal-portal
- `python_gist_task.py`: we have changed the code to do the pre-installation steps required to have `pyleargist` installed on the Azure Batch node plus other python package dependencies
- `python_gist_task.py`: creates now as an output a json file to the chosen Azure Storage container


## Usage
### Note: Before you run the below command you will need to modify accordingly its global variables/parameters 


`python python_gist_client.py`
