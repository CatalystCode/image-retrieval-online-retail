import os
import numpy as np
#import cv2
import imageio
import azure.storage.blob as azureblob


_STORAGE_ACCOUNT_NAME = 'yourstorageaccount'
_STORAGE_ACCOUNT_KEY = 'yourstorageaccountkey'
_STORAGE_INPUT_CONTAINER = 'yourinputcontainer'
_PREFIX_='prefix-virtual-folder/to-filter-interesting-files'
_PREFIX2_='-interactively-segmented_masked'
_SAVE_DIR='save_dir/'

'''
filepath=os.path.join(os.path.realpath('.'),'save_dir')
npy_path=os.path.join(filepath,'12031854sd.quick.npy')
msk_path=os.path.join(filepath,'12031854sd.quick.mask.gif')
'''

def list_files_in_container(blob_client,container,prefix):
    blobs = [blob.name for blob in blob_client.list_blobs(container) if prefix in blob.name and 'npy' in blob.name]
    return blobs

def download_blob(blob_client, container,prefix,savedir, blob):
    blob_client.get_blob_to_path(container_name=container, blob_name= blob, file_path= os.path.join(savedir,blob.replace(prefix,'')))
    return savedir+blob.replace(prefix,'')

def upload_blob(blob_client, container, blobname, localfile):
    blob_client.create_blob_from_path(container,
                                    blobname,
                                    localfile)

def npy_to_gif(npy_array_path, gif_path):
    np_array=np.load(npy_array_path)*255
    imageio.imsave(gif_path,np_array)
    np_array2=imageio.imread(gif_path)

if __name__ == '__main__':
    #npy_to_gif(npy_path,msk_path)
    blob_client = azureblob.BlockBlobService(
        account_name=_STORAGE_ACCOUNT_NAME,
        account_key=_STORAGE_ACCOUNT_KEY)
    blobs=list_files_in_container(blob_client, _STORAGE_INPUT_CONTAINER, _PREFIX_)
    print('got {} blobs'.format(len(blobs)))

    i=0
    for blob in blobs:
        i=i+1
        npy_array=download_blob(blob_client,_STORAGE_INPUT_CONTAINER,_PREFIX_,_SAVE_DIR,blob)
        local_file=npy_array.replace('.npy','.mask.gif').replace(_PREFIX2_,'')
        npy_to_gif(npy_array, local_file)
        blob_name=_PREFIX_+local_file.replace(_SAVE_DIR,'')
        upload_blob(blob_client,_STORAGE_INPUT_CONTAINER,blob_name,local_file)
        os.remove(npy_array)
        os.remove(local_file)
        print('progress {} out of {}'.format(i,len(blobs)))






    

