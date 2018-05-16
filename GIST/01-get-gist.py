### GIST requires python 2
### Consider parallsim

import os
import cPickle as pickle
import gistutils

def get_color_gist_for_images(datapath, outfile):
    print('get_color_gist_for_images ...')

    # create colour gist
    imgdict = gistutils.create_color_gist(datapath)

    # pickle the output
    with open(datapath + outfile, 'wb') as handle:
        pickle.dump(imgdict, handle, protocol=pickle.HIGHEST_PROTOCOL)
        print(outfile + " is dumped")
        
    # delete imgdict
    del imgdict
    
def resize_mobile_to_catalogue(datapath):
    print('resize_mobile_to_catalogue.....')
    from PIL import Image
    
    f_list = [os.path.join(root,f) for root, directories, filenames in os.walk(datapath) for f in filenames]
    #print('len(f_list): {0}'.format(len(f_list)))
    
    for f in f_list:
        #print(f)
        img = Image.open(f)
        img = img.resize((1571,2000), Image.ANTIALIAS) # (1571,2000) size of catalogue image, original mobile size (3024, 4032)
        img.save(f) 

if __name__ == "__main__":
    
    # consider resizing mobile images to smaller size
    # - for faster GIST computation
    # - cosine distance take same size vector (GIST will return same size vector)
    # resize_mobile_to_catalogue()
      
    # Get GIST from catalogue images
    get_color_gist_for_images('/your/catalogue/images/', 'catalogue_image_gist.pickle')
    
    # Get GIST from catalogue images
    get_color_gist_for_images('/your/segmented_mobile/images/', 'mobile_image_gist.pickle')
    