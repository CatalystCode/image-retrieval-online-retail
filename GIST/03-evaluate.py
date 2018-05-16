import cPickle as pickle
import gistutils
import csv

def do_evaluate(datapath, distpath, outfile):
    print('do_evaluate ... ')
    
    # load mobile distance_dict_scenario1
    with open(distpath, 'rb') as handle:
        distance_dict = pickle.load(handle)
        print(len(distance_dict))
    
    correct_match_dict = gistutils.evaluate(distance_dict, mode='CATRETRIEVAL')
    print(len(correct_match_dict))
    
    # count the matches
    # add 1 if correct_match_dict value is 1, i.e. the top 1st match is in the same folder
    
    correct_match_percent = []

    # if mode='CATRETRIEVAL'
    for topn in range(0,10):
        correct_match_percent.append(sum(1 for x in correct_match_dict.values() if x[0]<=topn)/float(len(distance_dict)))

    print(correct_match_percent)
     
    if True: # True to write out
        with open(outfile, 'wb') as f:
            writer = csv.writer(f, delimiter='\t')
            writer.writerow(correct_match_percent)
    
if __name__ == "__main__":
    
    # example usage
    datapath = '/your/catalogue/images/' 
    distpath = datapath + 'distance_dict_mobile_vs_catalogue_euclidean.pickle' # sorted distance per mobile image
    outfile = datapath + 'correct_match_percent_gist_mobile_vs_catalogue_euclidean.tsv' # output percentage of accurate retrieval from top nth closest match
    do_evaluate(datapath, distpath, outfile)
    