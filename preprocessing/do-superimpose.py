from PIL import Image
import numpy as np
import cv2
    
def naive_mask(mask):
    ''' 
    Take an image of relatively homogenous background and turn into a mask.
    return a boolean mask and a boolean inversed mask
    '''
    import numpy as np
    
    r = mask[:,:,0]
    g = mask[:,:,1]
    b = mask[:,:,2]

    mask_bool = np.empty(mask.shape, dtype=bool)
    mask_bool_inv = np.empty(mask.shape, dtype=bool)

    mask_bool[:,:,0] = r==255 # True
    mask_bool[:,:,1] = g==255 # True
    mask_bool[:,:,2] = b==255 # True

    mask_bool_inv = np.invert(mask_bool)
            
    return mask_bool, mask_bool_inv

def otsu_mask(mask):
    ''' 
    Take an image of relatively homogenous background and turn into a mask using Otsu's Thresholding.
    return a boolean mask and a boolean inversed mask
    '''
    import numpy as np
    import cv2

    gray = cv2.cvtColor(mask,cv2.COLOR_BGR2GRAY)
    # Otsu's thresholding after Gaussian filtering
    blur = cv2.GaussianBlur(gray,(5,5),0)
    ret,thresh = cv2.threshold(blur,0,255,cv2.THRESH_BINARY+cv2.THRESH_OTSU) # a height x width, 2D

    # positive mask
    mask_bool = thresh == 255 
    mask_bool = np.dstack((mask_bool, mask_bool, mask_bool))

    # negative mask
    mask_bool_inv = np.invert(mask_bool)    
    
    return mask_bool, mask_bool_inv

def superimpose_images(foreground_path, background_path, mode):
    import numpy as np
    import cv2

    foreground = cv2.imread(foreground_path); 
    background = cv2.imread(background_path); 
    background = cv2.resize(background, (foreground.shape[1], foreground.shape[0])); 
    ###
    # OpenCV uses BGR as its default colour order for images, matplotlib uses RGB. 
    # When you display an image loaded with OpenCv in matplotlib the channels will be back to front.
    # The easiest way of fixing this is to use OpenCV to explicitly convert it back to RGB
    ###
    foreground = cv2.cvtColor(foreground, cv2.COLOR_BGR2RGB)
    background = cv2.cvtColor(background, cv2.COLOR_BGR2RGB)
        
    # use foreground as mask, in this case catalogue
    mask = foreground

    if mode == 'naive':
        mask_bool, mask_bool_inv = naive_mask(mask); 
    elif mode == 'otsu':
        mask_bool, mask_bool_inv = otsu_mask(mask); 
    
    # get the foreground, in this case is the object of interest in catalogue
    fg = foreground * mask_bool_inv
    # get the background, in this case is a background image
    bg = background * mask_bool
    # add 
    superimposed = fg + bg
    
    return superimposed

if __name__ == "__main__":
    import glob
    import os
    
    base_dir = '/your/folder/'
    bg_list = sorted(glob.glob(base_dir +'your_background_folder/*.jpg'))
    fg_list = glob.glob(base_dir +'your_foreground_folder/*.jpg')
    out_dir = '/your_output_folder/'
    
### DO SUPERIMPOSE ###
    
    for bgf in bg_list:
        for fgf in fg_list:
            superimposed = superimpose_images(fgf, bgf, mode='otsu')

            if False: # True to save
                im = Image.fromarray(superimposed)
                outname = out_dir + os.path.basename(fgf)[:-4] + '_' + os.path.basename(bgf) 
                im.save(outname)

    