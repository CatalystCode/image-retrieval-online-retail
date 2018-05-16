'''
from PIL import Image
import glob
import os
from scipy import spatial
from operator import itemgetter
from scipy import spatial
import cPickle as pickle
'''
from __future__ import print_function
import argparse
import os
import azure.storage.blob as azureblob
import numpy
import leargist
from PIL import Image
import json


def create_color_gist(f_list):    
    imgdict = {}
    for i in range(len(f_list)):
        # load the image and compute the descriptors
        imagePath = f_list[i]
        image = Image.open(imagePath)
        h = leargist.color_gist(image)
        # update the database
        # key: image Path, value: descriptors
        imgdict[imagePath.replace('_','/')]= h.tolist() #needed to then serialize to json
        i=i+1
        if (i%10 == 0):
            print("processed %i images" % i)
    return(imgdict)
    
if __name__ == '__main__':
   
    parser = argparse.ArgumentParser()
    parser.add_argument('--filedir', required=True,
                        help='The dir name of images to process. The path'
                             'may include a compute node\'s environment'
                             'variables, such as'
                             '$AZ_BATCH_NODE_SHARED_DIR/filename.txt')
    parser.add_argument('--storageaccount', required=True,
                        help='The name of the Azure Storage account that owns the'
                             'blob storage container to which to upload'
                             'results and from which to download images.')
    parser.add_argument('--storagecontainer', required=True,
                        help='The Azure Blob storage container to which to'
                             'upload results.')
    parser.add_argument('--sastoken', required=True,
                        help='The SAS token providing write access to the'
                             'Storage container.')
    parser.add_argument('--inputcontainer', required=True,
                        help='The Azure Blob storage container from which to'
                             'download results.')
    parser.add_argument('--accountkey', required=True,
                        help='The SAS token providing write access to the'
                             'Storage container.')
    args = parser.parse_args()

    filedir = args.filedir
    
    
    # Create the blob client using the input container's SAS token.
    # This allows us to create a client that provides read
    # access only to the container.
    #blob_client_out = azureblob.BlockBlobService(account_name=args.storageaccount,
    #                                         sas_token=args.sastoken)

    blob_client = azureblob.BlockBlobService(
        account_name=args.storageaccount,
        account_key=args.accountkey)

    blobs=blob_client.list_blobs(container_name=args.inputcontainer, prefix=filedir)    
    
    blobs_to_process = [blob.name for blob in blobs if '.jpg' in blob.name ]

    for blob in blobs_to_process:
        blob_client.get_blob_to_path(container_name=args.inputcontainer, blob_name= blob, file_path= blob.replace('/','_'))
    
    local_file_names=[blob.replace('/','_') for blob in blobs_to_process]
    
    imgdict={}
    #imgdict['pippo']='pluto'
    imgdict = create_color_gist(local_file_names)


    # FINALLY
    output_file = 'gist_out_{}.json'.format(filedir.replace('/','')) # needs to be the output dict, 
    # needs to have no headers to append to other results later
    # and the reference to the image something like 0000/filename.jpg
    
    with open(output_file, 'w') as file:
       file.write(json.dumps(imgdict))
    
    '''
    with open(output_file, "w") as text_file:
        print("------------------------------", file=text_file)
        print("Node: " + os.environ['AZ_BATCH_NODE_ID'], file=text_file)
        print("Task: " + os.environ['AZ_BATCH_TASK_ID'], file=text_file)
        print("Job:  " + os.environ['AZ_BATCH_JOB_ID'], file=text_file)
        print("Pool: " + os.environ['AZ_BATCH_POOL_ID'], file=text_file)
    '''

    # Create the blob client using the output container's SAS token.
    # This allows us to create a client that provides write
    # access only to the container.
    blob_client = azureblob.BlockBlobService(account_name=args.storageaccount,
                                             sas_token=args.sastoken)

    output_file_path = os.path.realpath(output_file)

    print('Uploading file {} to container [{}]...'.format(
        output_file_path,
        args.storagecontainer))

    blob_client.create_blob_from_path(args.storagecontainer,
                                      output_file,
                                      output_file_path)


    