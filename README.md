PLEASE NOTE THAT IF THE SNAKE STANDS STILL AND DOES NOT MOVE,
THE PROGRAM RUNS, THE MODEL IS TRYING TO MOVE THE SNAKE ON TOP OF ITSELF,
WE PREVENT THIS IN THE C# SCRIPT SnakeAgent.cs IN UNITY.


1.) Environment Setup:

Make sure you have:

Unity Version 2018.2.18f1 Personal Edition
Python 3.6.6 and Jupyter Notebook (latest version)
Spyder with IPython 6.5.0

Tensorflow and tensorboard (GPU or CPU) we use GPU, latest version works.

WARNING: TENSORBOARD WILL REVERT TO AN OLDER VERSION WHEN INSTALLING MLAGENTS Python package!

ML-AGENTS DEPENDENCIES INSTALL THESE FIRST!!!!:
https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Installation.md
https://github.com/Unity-Technologies/ml-agents/blob/master/ml-agents/requirements.txt

tensorflow==1.7.1 * NOTE THIS WORKS WITH LATER VERSIONS IM ON 12.x
Pillow>=4.2.1
matplotlib
numpy>=1.11.0
jupyter
pytest>=3.2.2
docopt
pyyaml
protobuf==3.6.0
grpcio==1.11.0
Keras (GPU or CPU latest version works well)

then in Anaconda command prompt use PIP to install mlagents 0.5.0
https://pypi.org/project/mlagents/
pip install mlagents

2.) HOW TO RUN:
Startup Unity Personal Editor
Startup Spyder

In Spyder open testRun.py in the SnakeDeepLearning/Assets/Scripts/Python

Run the testRun.py in Spyder first watch the output, and wait till it says
you can now press the play button on Unity Editor to start.

Press the play button on the Unity Editor (looks like a play button)

Watch as the snake plays the game in the Editor.

Please note that logs file in SnakeDeepLearning/Assets/Scripts/Python is the event logging for
Tensorboard based on our KERAS model of RNN.
