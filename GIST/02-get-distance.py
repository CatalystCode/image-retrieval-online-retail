import cPickle as pickle
import gistutils

def get_distance(gist_path1, gist_path2, outpath, metric, norm):
    print('get_distance...')
    
    # load catalogue gist
    with open(gist_path1, 'rb') as handle:
        catalogue_gist_dict = pickle.load(handle)

    # load mobile gist
    with open(gist_path2, 'rb') as handle:
        mobile_gist_dict = pickle.load(handle)
        
    # compute distance
    distance_matrix_dict, distance_dict = gistutils.compute_distance(mobile_gist_dict, catalogue_gist_dict, metric=metric, norm=norm)
    
    if True: # True if want to write out
    
        # write out distance matrix
        outfile = outpath + 'distance_matrix_dict_mobile_vs_catalogue_' + metric + '.pickle'

        with open(outfile, 'wb') as handle:
            pickle.dump(distance_matrix_dict, handle, protocol=pickle.HIGHEST_PROTOCOL)
            print(outfile + ' is dumped ... ')

        # write out sorted distance per mobile image
        outfile = outpath + 'distance_dict_mobile_vs_catalogue_' + metric + '.pickle'

        with open(outfile, 'wb') as handle:
            pickle.dump(distance_dict, handle, protocol=pickle.HIGHEST_PROTOCOL)  
            print(outfile + ' is dumped ... ')

if __name__ == "__main__":
    
    gist_path1 = '/your/catalogue/gist/catalogue_image_gist.pickle' 
    gist_path2 = '/your/mobile/gist/mobile_image_foreground_segmented_gist.pickle'
    outpath = '/your/output/folder/'

    # some examples
    get_distance(gist_path1, gist_path2, outpath, metric='euclidean', norm=None)
    get_distance(gist_path1, gist_path2, outpath, metric='euclidean', norm='L1')
    get_distance(gist_path1, gist_path2, outpath, metric='minkowski', norm=None)
    