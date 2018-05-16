from PIL import Image
import glob
from scipy import spatial
from operator import itemgetter
import cPickle as pickle

def create_color_gist(datapath):
    import leargist
    
    # get the list of images
    f_list = glob.glob(datapath + '*.jpg*')
    
    imgdict = {}

    for i in range(len(f_list)):
        # load the image and compute the descriptors
        imagePath = f_list[i]
        image = Image.open(imagePath)
        h = leargist.color_gist(image)
        # update the database
        # key: image Path, value: descriptors
        imgdict[imagePath]= h

        # print out progress
        i=i+1
        if (i%1 == 0):
            print("processed %i images" % i)

    #print(len(imgdict))
    return(imgdict)

def l1_norm(v):
    import numpy as np
    return v/np.sum(v)

def compute_distance(imgdict1, imgdict2, metric='euclidean', norm=None):
    from collections import defaultdict

    "len(imgdict1.keys()) of query: {0}, len(imgdict2.keys()) of reference: {1} ".format(len(imgdict1.keys()), len(imgdict2.keys()))

    # declare a defaultdict to be used as 2D distance_matrix_dict: to store pairwise distance
    distance_matrix_dict = defaultdict(dict)

    # declare a defaultdict to be used as 1D distance_dict: to store top n closest (image, distance)
    distance_dict = defaultdict(dict)

    for imh1 in imgdict1.keys():
        for imh2 in imgdict2.keys():
            if metric == 'euclidean':
                if norm == 'L1':
                    distance_matrix_dict[imh1][imh2] = spatial.distance.euclidean(l1_norm(imgdict1[imh1]), l1_norm(imgdict2[imh2]))
                else:
                    distance_matrix_dict[imh1][imh2] = spatial.distance.euclidean(imgdict1[imh1], imgdict2[imh2])
                    
            elif metric == 'cosine':
                distance_matrix_dict[imh1][imh2] = spatial.distance.cosine(imgdict1[imh1], imgdict2[imh2])
            
            elif metric == 'hamming':
                distance_matrix_dict[imh1][imh2] = spatial.distance.hamming(imgdict1[imh1], imgdict2[imh2])
            
            elif metric == 'minkowski':
                distance_matrix_dict[imh1][imh2] = spatial.distance.minkowski(imgdict1[imh1], imgdict2[imh2])

        distance_dict[imh1] =sorted(distance_matrix_dict[imh1].items(), key=itemgetter(1))

    "len(distance_dict) : {0}, len(distance_matrix_dict) : {1}".format(len(distance_dict), len(distance_matrix_dict))

    # distance_dict[imagename][i] to access the closest image, i.e. itself
    # e.g. distance_dict['/yourdirectory/yourimage00.jpg'][0] to access the closest image, i.e. itself
    
    return distance_matrix_dict, distance_dict

def evaluate(distance_dict, mode):
    # find the closest correct match
    # put the top match index in a dict

    correct_match_dict = {}

    for k, v in distance_dict.items():
        #print(k, v)
        
        if mode == 'CATCLEANING': # Catalogue cleaning
            # return index where the same folder name occur, except for itself
            # k[25:-14] is subfolder name, e.g. /91/, /90/ ...
            top_matched_index = [v.index(item) for item in v if k[25:-14] in item[0] # index of correctly closest match
                                 if item[0]!=k] # except for itself

        elif mode == 'CATRETRIEVAL': # Retriev n closest matches from catalogue, given mobile image
            # return index where the same folder name occur, except for itself
            top_matched_index = [v.index(item) for item in v if k[73:84] in item[0]] # index of correctly closest match

        correct_match_dict[k] = top_matched_index

    return correct_match_dict
    
if __name__ == "__main__":
    main()
    
