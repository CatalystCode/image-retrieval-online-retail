# Setup instructions for pyleargist
Following are the instructions for Ubuntu. We could not successfully install the package on Windows.

## Installation on Ubuntu Server 16.04
1. First perform a system update

`sudo apt-get update` 

2. Install fftw3 (http://www.fftw.org/) and python with development headers

(Note that pyleargist only works on Python 2.7)

`sudo apt-get install fftw3 fftw3-dev python2.7-dev`

3. Install pyleargist

Note: simply running `pip install pyleargist` would not work as a file is missing (namely src/pyleargist.pxd) from the official repository on pypi. You might want to point directly to bitbucket or you might do as follows. 

`curl -fSsL https://bootstrap.pypa.io/get-pip.py | python`

`pip install Cython numpy pillow`

`curl -O https://pypi.python.org/packages/f7/4a/2eef58a73c48aec6aca09254ef0f39148fd39b8dc7ec96d6b39d513b03eb/pyleargist-2.0.5.tar.gz`

`tar -xf pyleargist-2.0.5.tar.gz`

`cd pyleargist-2.0.5/src/`

`rm leargist.pyx`

`curl -O https://raw.githubusercontent.com/ryubidragonfire/image-similarity/master/dependencies/pyleargist-2.0.5/src/leargist.pyx?token=AHkRmiT8fDyY-dIUE07kBrwzUeuJz-hLks5aWAXTwA%3D%3D leargist.pyx`

`curl –O https://bitbucket.org/ogrisel/pyleargist/raw/8024021a0d229ed1e1459a5d6d1700da4aee28b1/src/leargist.pxd`

`cd ..`

`python setup.py build_ext` (use options -I and -L if you did not install fftw3 in /usr/local)

`python setup.py build`

`sudo python setup.py install`


 #### Issues:
 ```
 (root) user@host:~/git/image-similarity/dependencies/pyleargist-2.0.5$ sudo python setup.py install
Traceback (most recent call last):
  File "setup.py", line 3, in <module>
    from Cython.Distutils import build_ext
ImportError: No module named Cython.Distutils
```
Solution:
Your sudo is not getting the right python. This is a known behaviour of sudo in Ubuntu. Use the full path:
Find out your python path:

`which python`

Then,

`sudo /your/full/path/for/python/python setup.py install`