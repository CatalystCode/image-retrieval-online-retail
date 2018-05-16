def segment_foreground(img_path, rect):
    import numpy as np
    import cv2
    
    img = cv2.imread(img_path)
    ###
    # OpenCV uses BGR as its default colour order for images, matplotlib uses RGB. 
    # When you display an image loaded with OpenCv in matplotlib the channels will be back to front.
    # The easiest way of fixing this is to use OpenCV to explicitly convert it back to RGB
    ###
    img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    
    mask = np.zeros(img.shape[:2],np.uint8)

    bgdModel = np.zeros((1,65),np.float64)
    fgdModel = np.zeros((1,65),np.float64)

    rect = rect #(start_x, start_y, width, height)
    
    _ = cv2.grabCut(img,mask,rect,bgdModel,fgdModel,3,cv2.GC_INIT_WITH_RECT) # 3 iterations
    mask2 = np.where((mask==2)|(mask==0),0,1).astype('uint8')
    img_black = img*mask2[:,:,np.newaxis]

    ## Black background to white background
    #Get the background
    background = img - img_black
    #Change all pixels in the background that are not black to white 
    background[np.where((background > [0,0,0]).all(axis = 2))] =[255,255,255] 
    #Add the background and the image 
    img_white = background + img_black

    return img_black, img_white, mask2

if __name__ == "__main__":
    import glob
    import os
    import numpy as np

    base_dir = '/your/data/folder/'
    img_list = sorted(glob.glob(base_dir +'folder/images/to-be-processed/*.jpg'))
    rect = (10,0,496,516) #(start_x, start_y, width, height), image size is 516 x 516

    print(len(img_list))
    total = len(img_list)
    progress = 0
    
    for fname in img_list:
        # segmented is a segmented image on white background
        _, segmented, _ = segment_foreground(fname, rect)
        
        # mask2 is the mask in numpy array
        _, _, mask2 = segment_foreground(fname, rect)
        
        if True:
            # save segmented image
            im = Image.fromarray(segmented)
            outname = '/your/output/folder/' + os.path.basename(fname)
            im.save(outname)
            
            outname_np = '/your/output/folder/' + os.path.basename(fname)[:-3] + 'npy'; #print(outname)
            np.save(outname_np, mask2)
            
            # report progress every 10 images completed
            progress = progress + 1
            
            if progress%10 == 0:
                print('Progress: ' + str(progress/total))
                #print(outname)
                print(outname_np)